
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class SceneConductorService : MonoBehaviour, ISceneConductorService
{
    private IFadeController _fade;
    private IEventBus _bus;

    // simple sequential work queue to avoid overlapping loads
    private readonly Queue<Func<CancellationToken, Task>> _work = new();
    private CancellationTokenSource _cts;
    private bool _isRunning;

    // Subscriptions
    private IDisposable _subLoad;
    private IDisposable _subAdditive;
    private IDisposable _subUnload;
    private IDisposable _subReload;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // If not injected by DI, try to find a bus in scene (adjust to your project)
        if (_bus == null) _bus = ServiceLocator.Get<IEventBus>(); // placeholder; replace with your locator
        if (_fade == null) _fade = ServiceLocator.Get<IFadeController>();

        if (_bus == null)
            Debug.LogWarning("[SceneConductorService] No IEventBus found. Service will not react to commands.");
    }

    private void OnEnable()
    {
        if (_bus == null) return;

        _subLoad     = _bus.Subscribe<LoadSceneCommand>(On);
        _subAdditive = _bus.Subscribe<LoadAdditiveSceneCommand>(On);
        _subUnload   = _bus.Subscribe<UnloadSceneCommand>(On);
        _subReload   = _bus.Subscribe<ReloadCurrentSceneCommand>(On);
    }

    private void OnDisable()
    {
        _subLoad?.Dispose();     _subLoad = null;
        _subAdditive?.Dispose(); _subAdditive = null;
        _subUnload?.Dispose();   _subUnload = null;
        _subReload?.Dispose();   _subReload = null;
    }

    private void On(LoadSceneCommand cmd)
    {
        Enqueue(async ct => await LoadSingle(cmd.Scene.ToString(), cmd.WithFade, ct));
    }

    private void On(LoadAdditiveSceneCommand cmd)
    {
        Enqueue(async ct => await LoadAdditive(cmd.Scene.ToString(), cmd.ActivateOnLoad, ct));
    }

    private void On(UnloadSceneCommand cmd)
    {
        Enqueue(async ct => await Unload(cmd.Scene.ToString(), ct));
    }

    private void On(ReloadCurrentSceneCommand cmd)
    {
        Enqueue(async ct => await Reload(cmd.WithFade, ct));
    }

    public Task LoadSingle(string sceneName, bool withFade = true, CancellationToken ct = default)
        => RunOperation(async token =>
        {
            if (withFade && _fade != null) await _fade.FadeOut(ct: token);

            // Reset scoped services before loading new scene
            var serviceContainer = ServiceLocator.Get<IServiceContainer>();
            serviceContainer?.ResetScopedInstances();
            Debug.Log("Reset scoped service instances for scene change");

            var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            async.allowSceneActivation = true;
            while (!async.isDone)
            {
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (withFade && _fade != null) await _fade.FadeIn(ct: token);
        }, ct);

    public Task LoadAdditive(string sceneName, bool activateOnLoad = true, CancellationToken ct = default)
        => RunOperation(async token =>
        {
            var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            async.allowSceneActivation = activateOnLoad;
            while (!async.isDone)
            {
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (activateOnLoad)
            {
                var scn = SceneManager.GetSceneByName(sceneName);
                if (scn.IsValid())
                    SceneManager.SetActiveScene(scn);
            }
        }, ct);

    public Task Unload(string sceneName, CancellationToken ct = default)
        => RunOperation(async token =>
        {
            if (!SceneExists(sceneName))
                return;

            var async = SceneManager.UnloadSceneAsync(sceneName);
            if (async == null) return;

            while (!async.isDone)
            {
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }, ct);

    public Task Reload(bool withFade = true, CancellationToken ct = default)
        => RunOperation(async token =>
        {
            var current = SceneManager.GetActiveScene().name;
            if (withFade && _fade != null) await _fade.FadeOut(ct: token);

            var async = SceneManager.LoadSceneAsync(current, LoadSceneMode.Single);
            async.allowSceneActivation = true;
            while (!async.isDone)
            {
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (withFade && _fade != null) await _fade.FadeIn(ct: token);
        }, ct);


    private void Enqueue(Func<CancellationToken, Task> op)
    {
        _work.Enqueue(op);
        if (!_isRunning) _ = Pump();
    }

    private async Task Pump()
    {
        _isRunning = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            while (_work.Count > 0)
            {
                var op = _work.Dequeue();
                try
                {
                    await op(_cts.Token);
                }
                catch (OperationCanceledException) { /* ignored on cancel */ }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    private Task RunOperation(Func<CancellationToken, Task> body, CancellationToken external)
    {
        // If you need to combine external CT with the queue CT, you could link them.
        return body(_cts?.Token ?? external);
    }

    private static bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName) return true;
        }
        return false;
    }

}