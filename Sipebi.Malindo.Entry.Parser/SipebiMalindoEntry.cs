using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sipebi.Malindo.Entry.Parser {
	[Serializable]
	public class SipebiMalindoEntry {

		//ID [TAB] Akar [TAB] Bentuk lahir [TAB] Awalan/proklitik [TAB] Akhiran/enklitik
		//[TAB] Apitan [TAB] Penggandaan [TAB] Sumber [TAB] Dasar [TAB] Lema

		public string ID { get; set; }
		public string Akar { get; set; }
		public string BentukLahir { get; set; }
		public string Awalan { get; set; }
		public string Akhiran { get; set; }
		public string Apitan { get; set; }
		public string Penggandaan { get; set; }
		public string Sumber { get; set; }
		public string Dasar { get; set; }
		public string Lema { get; set; }
	}
}
