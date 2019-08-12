using Game.Network;

namespace Test
{
    class StringSender : MsgSender
    {
        public StringSender(string str)
        {
            _obj = str;
        }
        protected override byte[] Serialize(object obj)
        {
            string str = obj as string;
            return System.Text.Encoding.Default.GetBytes(str);
        }
    }
    class StringReceiver : MsgReceiver
    {
        protected override object Deserialize(byte[] bin)
        {
            return System.Text.Encoding.Default.GetString(bin);
        }
    }
}
