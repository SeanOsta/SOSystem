namespace Data
{
    public interface IGetData<T>
    {
        public T data { get; }
    }

    public interface ISetData<T>
    {
        public T data { set; }
    }

    public interface IData<T> : IGetData<T>, ISetData<T> { }
}