namespace System
{
    // Base to the model type of class. Mainly used to auto generate IDs for models.
    // Make sure to inherit BaseModel's constructor otherwise auto ID will not work.
    public class BaseModel
    {
        private static long idIncrementer = 1;
        public long ID { get; set; }

        public BaseModel(){
            this.ID = idIncrementer;
            idIncrementer++;
        }
    }
}