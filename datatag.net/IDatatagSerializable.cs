namespace Datatag
{
    public interface IDatatagSerializable
    {
        public Node Serialize();

        public void Deserialize(Node node);
    }
}