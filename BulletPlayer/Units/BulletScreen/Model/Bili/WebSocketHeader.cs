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
        public readonly byte[] headerPackageSize = new byte[4];
        public readonly byte[] headerHeaderSize = new byte[2];
        public readonly byte[] headerProtocol = new byte[2];
        public readonly byte[] headerOperation = new byte[4];
        public readonly byte[] headerSequence = new byte[4];

        public WebSocketHeader(int packageSize,int headerSize,int protocol,int operation,int sequence)
        {
            headerPackageSize = MyConverter.IntToBytes(packageSize);
            headerHeaderSize = MyConverter.IntToBytes(headerSize);
            headerProtocol = MyConverter.IntToBytes(protocol);
            headerOperation = MyConverter.IntToBytes(operation);
            headerSequence = MyConverter.IntToBytes(sequence);
        }

        public byte[] GetHeaderConcat()
        {
            return headerPackageSize.Concat(headerHeaderSize).Concat(headerProtocol).Concat(headerOperation)
                .Concat(headerSequence).ToArray();
        }
    }
}
