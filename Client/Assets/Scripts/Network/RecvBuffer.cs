using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;

        // r,w차이가 패킷만큼 이상 일 때 처리함, r은 처리하는 경우 위치 갱신, w는 패킷의 일부가 오면 위치 갱신
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize],0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if(dataSize == 0)
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                //아직 처리하지 않은 부분 데이터가 있는 경우
                Array.Copy(_buffer.Array,_buffer.Offset+_readPos, _buffer.Array, _buffer.Offset, DataSize);
                _readPos = 0;
                _writePos = dataSize;

            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize) return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if(numOfBytes > FreeSize) return false;

            _writePos += numOfBytes;
            return true;
        }

    }
}
