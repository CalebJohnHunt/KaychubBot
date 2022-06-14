using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * 63-56 0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000
 * 55-48 0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000
 * 47-40 0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000
 * 39-32 0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000
 * 31-24 0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000
 * 23-16 0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000
 * 15-08 0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000   0000 0000
 * 07-00 0000 0000   0000 0000   0000 0000   0000 0000   door show   door used   car loc     step
 */

namespace TelegramBot
{
    internal class QueryData : ICloneable
    {
        byte[] bytes;
        const byte STEP = 0;
        const byte CARLOC = 1;
        const byte DOORUSED = 2;
        const byte DOORSHOWN = 3;
        const byte CHOSESTAY = 4;
        public byte step => bytes[STEP];
        public byte carLoc => bytes[CARLOC];
        public byte doorUsed => bytes[DOORUSED];
        public byte doorShown => bytes[DOORSHOWN];
        public bool choseStay => bytes[CHOSESTAY] == 1;
        
        public QueryData(string data)
        {
            bytes = Encoding.Latin1.GetBytes(data);
            if (bytes.Length != 64)
                throw new ArgumentException("QueryData constructor takes a string with 64 bytes", nameof(data));
        }
        
        public QueryData(byte[] bytes)
        {
            this.bytes = bytes;
        }
        
        public QueryData()
        {
            bytes = new byte[64];
        }
        public QueryData Step(byte b)
        {
            bytes[STEP] = b;
            return this;
        }
        public QueryData CarLoc(byte b)
        {
            bytes[CARLOC] = b;
            return this;
        }
        public QueryData DoorUsed(byte b)
        {
            bytes[DOORUSED] = b;
            return this;
        }
        public QueryData DoorShown(byte b)
        {
            bytes[DOORSHOWN] = b;
            return this;
        }
        public QueryData ChoseStay(bool b)
        {
            bytes[CHOSESTAY] = (byte)(b ? 1 : 0);
            return this;
        }
        public override string ToString()
        {
            return Encoding.Latin1.GetString(bytes);
        }
        public object Clone() => new QueryData((byte[])this.bytes.Clone());
    }
}
