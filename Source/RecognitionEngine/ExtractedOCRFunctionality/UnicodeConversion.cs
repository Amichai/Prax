using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractedOCRFunctionality {
	public static class UnicodeConversion {
		private enum letterPosition {
			start, middle, end, isolated
		};

		static private char testForLigatures(int currentChar, int nextChar) {
			if (currentChar == 1604 && nextChar == 1570) {
				return (char)65269;
			}
			if (currentChar == 1604 && nextChar == 1571) {
				return (char)65271;
			}
			if (currentChar == 1604 && nextChar == 1573) {
				return (char)65273;
			}
			if (currentChar == 1604 && nextChar == 1575) {
				return (char)65275;
			}
			return char.MinValue;
		}

		private static HashSet<int> restrictedForms = new HashSet<int> { 1570, 1571, 1573, 1575, 1577, 1583, 1584, 1585, 1586, 1608, 1609 };

		private enum arabicLetterForms {
			restricted, unrestricted
		};

		private static arabicLetterForms previousForm = arabicLetterForms.restricted;

		public static char convertUnicodeChar(string word, ref int idx) {
			int currentChar = word[idx];
			if (currentChar > 65000)
				return (char)currentChar;
			arabicLetterForms currentForm;
			if (restrictedForms.Contains(currentChar))
				currentForm = arabicLetterForms.restricted;
			else
				currentForm = arabicLetterForms.unrestricted;

			letterPosition currentPosition = letterPosition.middle;

			//Check for symbols with no contextual forms
			if (word[idx] == 1569)
				return (char)65152;
			if (word[idx] == 1572)
				return (char)1572;


			if (idx == word.Count() - 1)
				currentPosition = letterPosition.end;
			if (idx == 0) {
				currentPosition = letterPosition.start;
				previousForm = arabicLetterForms.restricted;
			}
			if (word.Count() == 1)
				currentPosition = letterPosition.isolated;

			int newCharVal = getContextualForm(currentChar);
			if (newCharVal == char.MinValue)
				return char.MinValue;

			int nextChar = 0, prevChar = 0;

			if (idx > 0)
				prevChar = word[idx - 1];
			if (idx < word.Count() - 1)
				nextChar = word[idx + 1];

			//Special case, broken glyph. Specific to rendering in WORD
			if (newCharVal == 65263) {
				if (currentPosition == letterPosition.isolated) //stand alone letter
					return (char)newCharVal;
				if (currentPosition == letterPosition.start) {   //No right bind
					return (char)65267;
				}
				if (currentPosition == letterPosition.end) {  //Right bind only
					return (char)65264;
				}
				if (currentPosition == letterPosition.middle) { //Right and left bind
					return (char)65267;
				}
			}

			int ligature = testForLigatures(currentChar, nextChar);
			if (ligature != char.MinValue) {
				newCharVal = ligature;
				currentForm = arabicLetterForms.restricted;
				idx++;
			}

			if (currentPosition == letterPosition.isolated) //stand alone letter
			{
				newCharVal = newCharVal + 0; //isolated glyph
			} else {
				if (currentPosition == letterPosition.start || previousForm == arabicLetterForms.restricted) {   //No right bind
					if (currentForm == arabicLetterForms.restricted)
						newCharVal = newCharVal + 0; //isolated glyph
					if (currentForm == arabicLetterForms.unrestricted && currentPosition != letterPosition.end)
						newCharVal = newCharVal + 2; //starting glyph
				}

				if ((currentPosition == letterPosition.end || currentForm == arabicLetterForms.restricted) && previousForm == arabicLetterForms.unrestricted) {  //Right bind only
					newCharVal = newCharVal + 1;
				}
				if (currentPosition != letterPosition.end && currentForm == arabicLetterForms.unrestricted && previousForm == arabicLetterForms.unrestricted) { //Right and left bind
					newCharVal = newCharVal + 3;
				}
			}

			previousForm = currentForm;
			return (char)newCharVal;
		}

		static int getContextualForm(int currentChar) {
			switch (currentChar) {
				case 1574: return 65161;
				case 1575: return 65165;
				case 1576: return 65167;
				case 1578: return 65173;
				case 1579: return 65177;
				case 1580: return 65181;
				case 1581: return 65185;
				case 1582: return 65189;
				case 1583: return 65193;
				case 1584: return 65195;
				case 1585: return 65197;
				case 1586: return 65199;
				case 1587: return 65201;
				case 1588: return 65205;
				case 1589: return 65209;
				case 1590: return 65213;
				case 1591: return 65217;
				case 1592: return 65221;
				case 1593: return 65225;
				case 1594: return 65229;
				case 1601: return 65233;
				case 1602: return 65237;
				case 1603: return 65241;
				case 1604: return 65245;
				case 1605: return 65249;
				case 1606: return 65253;
				case 1607: return 65257;
				case 1608: return 65261;
				case 1610: return 65265;
				case 1570: return 65153;
				case 1571: return 65155;
				case 1577: return 65171;
				case 1609: return 65263;
			}
			return char.MinValue;
		}
	}
}
