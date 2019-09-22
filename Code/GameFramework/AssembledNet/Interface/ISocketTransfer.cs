namespace AssembledNet
{
    public interface ISocketTransfer
    {
        void AddToSend(byte[] data);
        bool TryGetReceivedData(out byte[] data);
        SocketTransfer.TransferResult TryReceive();
        SocketTransfer.TransferResult TrySend();
    }
}