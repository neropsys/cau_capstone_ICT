using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Capston2
{
    public class EncodeUtf16ToUtf8
    {
        public static string Utf16ToUtf8(string utf16String)
        {

            string utf8String = String.Empty;



            //UTF-16 바이트를 배열로 얻어온다.

            byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);

            //UTF-16 바이트를 배열을 UTF-8로 변환한다.

            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);



            // UTF8 bytes 배열 내부에 UTF8 문자를 추가한다.

            for (int i = 0; i < utf8Bytes.Length; i++)

            {

                // Because char always saves 2 bytes, fill char with 0

                byte[] utf8Container = new byte[2] { utf8Bytes[i], 0 };

                utf8String += BitConverter.ToChar(utf8Container, 0);

            }



            // UTF8을 리턴한다.

            return utf8String;

        }
    }
}