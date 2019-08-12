using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Game.Utility;

namespace Game.Network
{
    public class SocketWrapper: SocketWrapperBase
    {
        private Socket _socket;
        private Thread _thread = null;
        private bool _receiving = false;

        public SocketWrapper(string host, int port) : base( host, port) { }

        /// <summary>
        /// 打开连接
        /// </summary>
        public override void Connect(Action<bool> completeListener)
        {
            Action<IAsyncResult> connectCallback = (asyncresult) =>
            {
                if (_socket != null)
                {
                    bool connected = _socket.Connected;
                    if (connected)
                    {
                        _receiving = true;
                        _thread = new Thread(new ThreadStart(checkReceive));
                        _thread.Start();
                    }
                    else
                    {
                        _socket = null;
                    }
                    if (completeListener != null)
                    {
                        completeListener(connected);
                    }
                }
            };
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            try
            {
                //_socket.Connect(_host, _port);
                _socket.BeginConnect(_host, _port, new AsyncCallback(connectCallback),_socket);
            }
            catch
            {
                //socket.Dispose();
                _socket = null;
                throw;
            }
        }
        
        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
        {
            if (_socket == null) return;
            LogHelper.LogInfo("Close Socket");
            try
            {
                lock (this)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;

                    //_thread.Abort();
                    _receiving = false;
                    _thread = null;
                    onClosed();
                }

            }
            catch (Exception ex)
            {
                _socket = null;
                onError(ex);
            }
        }

        /// <summary>
        /// 连接是否建立
        /// </summary>
        public override bool IsConnect
        {
            get { return _socket != null; }
        }

        private void checkReceive()
        {
            LogHelper.LogInfo("Receive thread start.");
            while (_receiving)
            {
                lock (this)
                {
                    if (_socket == null) return;
                    try
                    {
                        if (_socket.Poll(5, SelectMode.SelectRead))
                        {
                            if (_socket.Available == 0)
                            {
                                LogHelper.LogErr("Available = 0");
                                break;
                            }

                            //读取前4个字节（包长度）
                            byte[] prefix = new byte[4];
                            int preLength = 0;
                            do
                            {
                                int rev = _socket.Receive(prefix, preLength, 4 - preLength, SocketFlags.None);
                                if (rev <= 0)
                                {
                                    break;
                                }
                                preLength += rev;
                            } while (preLength != 4);

                            //int packetLength = BitConverter.ToInt32(prefix, 0);
                            //获取包长度(big endian)
                            int packetLength = StructHelper.GetBigEndianHead(prefix);
                            LogHelper.LogInfo("socket:{0} receive packet，长度：{1}", _socket.LocalEndPoint, packetLength);

                            byte[] packet = new byte[packetLength];
                            int receiveLength = 0;

                            //接受完整包
                            do
                            {
                                int rev = _socket.Receive(packet, receiveLength, packetLength - receiveLength, SocketFlags.None);
                                if (rev <= 0)
                                {
                                    break;
                                }
                                receiveLength += rev;
                            } while (receiveLength != packetLength);

                            ////判断流是否有Gzip压缩
                            //if (data[0] == 0x1f && data[1] == 0x8b && data[2] == 0x08 && data[3] == 0x00)
                            //{
                            //    data = NetReader.Decompression(data);
                            //}

                            handleReceiveData(packet);
                        }
                        else if (_socket.Poll(5, SelectMode.SelectError))
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        onError(ex);
                    }
                }
                Thread.Sleep(5);
            }
            LogHelper.LogInfo("Receive thread over.");
            Close();
        }


        private byte[] addPrefix(byte[] data)
        {
            Int32 length = (Int32)data.Length;
            //消息，前4位为表示消息长度的消息头
            byte[] result = new byte[length + 4];
            //big endian 消息头
            StructHelper.SetBigEndianHead(ref result, length);
            Buffer.BlockCopy(data, 0, result, 4, length);
            return result;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public override bool Send(byte[] data)
        {
            if (_socket != null)
            {
                try
                {
                    data = addPrefix(data);
                    //socket.Send(data);
                    IAsyncResult asyncSend = _socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendCallback), _socket);
                    bool success = asyncSend.AsyncWaitHandle.WaitOne(5000, true);
                    if (!success)
                    {
                        LogHelper.LogErr("asyncSend error close socket");
                        Close();
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.LogErr("asyncSend error:{0}",ex.ToString());
                    return false;
                }
            }
            else
            {
                LogHelper.LogErr("Not connectd!");
            }
            return false;
        }

        private void sendCallback(IAsyncResult asyncSend)
        {

        }
    }
}
