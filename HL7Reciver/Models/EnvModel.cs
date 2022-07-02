using System;
using System.Collections.Generic;

namespace HL7Reciver.Models
{
	public class EnvModel
	{
        public string ConnectionString { get; set; }
        public List<string> SendBack { get; set; }
        public int ControlIDIndex { get; set; }
    }
}

