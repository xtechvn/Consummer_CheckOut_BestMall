using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace HuloToys_Service.Utilities.lib
{
    public static class StringHelper
    {
        public static Regex regex_vietnamese_unicode = new Regex("[^a-zA-Z0-9àáãảạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệđùúủũụưừứửữựòóỏõọôồốổỗộơờớởỡợìíỉĩịäëïîöüûñçýỳỹỵỷÀÁÃẢẠĂẰẮẲẴẶÂẦẤẨẪẬÈÉẺẼẸÊỀẾỂỄỆĐÙÚỦŨỤƯỪỨỬỮỰÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢÌÍỈĨỊÄËÏÎÖÜÛÑÇÝỲỸỴỶ ]");
        public static Regex regex_email = new Regex("[^a-zA-Z0-9@_.]");
        public static bool HasSpecialCharacterExceptVietnameseCharacter(string text)
        {
            return regex_vietnamese_unicode.IsMatch(text);
        }
        public static string RemoveSpecialCharacterExceptVietnameseCharacter(string text)
        {
            return Regex.Replace(text, @"[^a-zA-Z0-9À-ỹĐđ\s]", "");
        }


        public static string NormalizeKeyword(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string noDiacritics = RemoveVietnameseAccents(text);
            return noDiacritics.Replace(" ", "").ToLower();
        }
        public static string RemoveVietnameseAccents(string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            return regex.Replace(formD, "")
                        .Replace('đ', 'd')
                        .Replace('Đ', 'D');
        }


        public static string RemoveSpecialCharacter(string text)
        {
            var pattern = new Regex("[^a-zA-Z0-9]");
            return pattern.Replace(text, "");
        }
        public static string RemoveSpecialCharacterUsername(string text)
        {
            return regex_email.Replace(text, "");
        }
        public static string ValidateTextForSearch(string input)
        {
            var validate= input.Normalize(NormalizationForm.FormC).ToLower().Trim();
            validate= regex_vietnamese_unicode.Replace(validate, "");

            return validate;
        }
        public static string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
    "đ",
    "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
    "í","ì","ỉ","ĩ","ị",
    "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
    "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
    "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
    "d",
    "e","e","e","e","e","e","e","e","e","e","e",
    "i","i","i","i","i",
    "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
    "u","u","u","u","u","u","u","u","u","u","u",
    "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }
        public static DateTime GetCurrentTimeInUtcPlus7(DateTime time)
        {
            // 1. Lấy DateTime.Now và chuyển đổi sang UTC
            DateTime utcNow = time.ToUniversalTime();

            // 2. Xác định múi giờ UTC+7
            TimeZoneInfo utcPlus7Zone;
            try
            {
                // Thử ID múi giờ phổ biến cho Windows
                utcPlus7Zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Nếu không tìm thấy (ví dụ: trên Linux), thử các ID IANA
                try
                {
                    // Sử dụng "Asia/Ho_Chi_Minh" hoặc "Asia/Bangkok" tùy thuộc vào địa điểm cụ thể bạn muốn.
                    // "Asia/Ho_Chi_Minh" là múi giờ của Việt Nam.
                    utcPlus7Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                }
                catch (TimeZoneNotFoundException)
                {
                    // Trường hợp xấu nhất không tìm thấy múi giờ nào, chúng ta nên có một fallback hợp lý.
                    // Hoặc throw exception để báo lỗi rõ ràng.
                    // Ở đây, chúng ta sẽ throw exception vì không thể xác định được múi giờ mong muốn.
                    throw new InvalidOperationException("Không thể tìm thấy múi giờ UTC+7 (SE Asia Standard Time hoặc Asia/Ho_Chi_Minh/Asia/Bangkok) trên hệ thống này.");
                }
            }

            // 3. Chuyển đổi từ UTC sang múi giờ UTC+7
            DateTime utcPlus7Time = TimeZoneInfo.ConvertTimeFromUtc(utcNow, utcPlus7Zone);

            // 4. Định dạng và trả về chuỗi
            return utcPlus7Time;
        }
    }
}
