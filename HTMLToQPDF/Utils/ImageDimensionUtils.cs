using System;

namespace HTMLToQPDF.Utils
{
    internal static class ImageDimensionUtils
    {
        public static bool TryGetDimensions(byte[] data, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (data is null || data.Length < 24)
                return false;

            if (IsPng(data))
                return TryReadPng(data, out width, out height);

            if (IsJpeg(data))
                return TryReadJpeg(data, out width, out height);

            return false;
        }

        private static bool IsPng(byte[] data) =>
            data.Length >= 8
            && data[0] == 0x89
            && data[1] == 0x50
            && data[2] == 0x4E
            && data[3] == 0x47;

        private static bool IsJpeg(byte[] data) =>
            data.Length >= 2 && data[0] == 0xFF && data[1] == 0xD8;

        private static bool TryReadPng(byte[] data, out int width, out int height)
        {
            width = 0;
            height = 0;
            // IHDR width/height start at byte 16
            if (data.Length < 24)
                return false;

            width = ReadBigEndianInt32(data, 16);
            height = ReadBigEndianInt32(data, 20);
            return width > 0 && height > 0;
        }

        private static bool TryReadJpeg(byte[] data, out int width, out int height)
        {
            width = 0;
            height = 0;
            var i = 2;
            while (i + 9 < data.Length)
            {
                if (data[i] != 0xFF)
                {
                    i++;
                    continue;
                }

                var marker = data[i + 1];
                if (marker == 0xD8 || marker == 0xD9)
                {
                    i += 2;
                    continue;
                }

                if (i + 3 >= data.Length)
                    break;

                var segmentLength = (data[i + 2] << 8) | data[i + 3];
                if (segmentLength < 2 || i + 1 + segmentLength >= data.Length)
                    break;

                // SOF0 / SOF1 / SOF2
                if (marker is 0xC0 or 0xC1 or 0xC2)
                {
                    height = (data[i + 5] << 8) | data[i + 6];
                    width = (data[i + 7] << 8) | data[i + 8];
                    return width > 0 && height > 0;
                }

                i += 2 + segmentLength;
            }

            return false;
        }

        private static int ReadBigEndianInt32(byte[] data, int offset) =>
            (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
    }
}
