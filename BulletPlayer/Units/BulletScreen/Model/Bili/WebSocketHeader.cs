using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletPlayer.Kits;

namespace BulletPlayer.Units.BulletScreen.Model.Bili
{
    public class WebSocketHeader
    {
        /// <summary>
        /// 封包总大小（头部大小+正文大小）L:4
        /// 
        /// </summary>
        public readonly int packageSize;

        /// <summary>
        /// 头部大小（一般为0x0010，16字节）L:2
        /// </summary>
        public readonly ushort headerSize;

        /// <summary>
        /// 协议版本 L:2
        /// 0 JSON纯文本
        /// 1 Body内容为房间人气值
        /// 2 压缩过的Buffer,Body内容需要用zlib.inflate解压出一个新的数据包,然后从数据包格式那一步重新操作一遍
        /// </summary>
        public readonly ushort protocol;

        /// <summary>
        /// 操作类型(封包类型) L:4
        /// 3 心跳回应 Body内容为房间人气值
        /// 5 通知     弹幕,广播等全部信息
        /// 8 进房回应
        /// </summary>
        public readonly int operation;

        /// <summary>
        /// sequence,可以取常数1 L:4
        /// </summary>
        public readonly int sequence;

        public WebSocketHeader(int packageSize, ushort headerSize, ushort protocol, int operation, int sequence)
        {
            this.packageSize = packageSize;
            this.headerSize = headerSize;
            this.protocol = protocol;
            this.operation = operation;
            this.sequence = sequence;
        }

        public WebSocketHeader(byte[] bytes)
        {
            packageSize = BitConverter.ToInt32(bytes.Take(4).Reverse().ToArray(), 0);
            headerSize = BitConverter.ToUInt16(bytes.Skip(4).Take(2).Reverse().ToArray(), 0);
            protocol = BitConverter.ToUInt16(bytes.Skip(6).Take(2).Reverse().ToArray(), 0);
            operation = BitConverter.ToInt32(bytes.Skip(8).Take(4).Reverse().ToArray(), 0);
            sequence = BitConverter.ToInt32(bytes.Skip(12).Take(4).Reverse().ToArray(), 0);
        }

        public byte[] GetHeaderConcat()
        {
            byte[] packageSizeBytes = MyConverter.IntToBytes(packageSize);
            byte[] headerSizeBytes = MyConverter.UshortToBytes(headerSize);
            byte[] protocolBytes = MyConverter.UshortToBytes(protocol);
            byte[] operationBytes = MyConverter.IntToBytes(operation);
            byte[] sequenceBytes = MyConverter.IntToBytes(sequence);

            return packageSizeBytes.Concat(headerSizeBytes).Concat(protocolBytes).Concat(operationBytes)
                .Concat(sequenceBytes).ToArray();
        }
    }
}
