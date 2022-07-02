using System;
using Mahas.Helpers;

namespace HL7Reciver.Models
{
    [DbTable("TPCListener")]
    public class ListenerModel
    {
        [DbKey(true)]
        [DbColumn]
        public int Id { get; set; }

        [DbColumn]
        public DateTime Tanggal { get; set; }

        [DbColumn]
        public string Pesan { get; set; }

        [DbColumn]
        public string SendBack { get; set; }

        [DbColumn]
        public bool isSuccess { get; set; }
    }
}
