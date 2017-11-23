using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadCenter
{
    class SyncResultRecords
    {
        #region // 紀錄處理物件
        private static List<SyncResult> _SyncResultList;
        public static List<SyncResult> All()
        {
            return _SyncResultList;
        }
        public static void Init()
        {
            _SyncResultList = null;
            _SyncResultList = new List<SyncResult>();
        }
        public static void Add(SyncResult obj)
        {
            if (_SyncResultList == null)
            {
                Init();
            }
            _SyncResultList.Add(obj);
        }
        public struct SyncResult
        {
            public string Id { get; set; }
            public string Size { get; set; }
            public string Status { get; set; }
            public string Message { get; set; }
            public string FinishTime { get; set; }
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }
        }
        #endregion

    }
}
