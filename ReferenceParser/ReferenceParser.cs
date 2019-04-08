using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReferenceParser
{

    /// <summary>
    /// The reference types recognised by the class. Unknown signifies that either no references have been 
    /// identified (PotentialReferences.count = 0) or that there are multiple possible references 
    /// (PotentialReferences.Count > 1)
    /// </summary>
    public enum DutyTypes
    {
        COTAX,
        CT,
        ITCP,
        MOSS,
        NTC,
        PAYE,
        SA,
        SAFE,
        SDLT,
        UNKNOWN
        //,NINO
    }

    public class ReferenceParser
    {

        List<string> m_PotentialReferences;
        DMBTools.GeneralTools m_GeneralTools = new DMBTools.GeneralTools();

        /// <summary>
        /// A list of possible references that are contained within the string
        /// </summary>
        public List<string> PotentialReferences
        {
            get { return m_PotentialReferences; }
        }

        DutyTypes m_IdentifiedDutyType;
        /// <summary>
        /// The head of duty for the identified reference.
        /// If no reference is identified (or there are multiple references) then this will be be showbn as
        /// Unknown
        /// </summary>
        public DutyTypes IdentifiedDutyType
        {
            get { return m_IdentifiedDutyType; }
        }

        public ReferenceParser()
        {
        }

        /// <summary>
        /// Checks a string to see if it contains any of the recognised reference types
        /// </summary>
        /// <param name="StringToCheck">The string to be checked</param>
        /// <returns>A list containing any valid references</returns>
        public List<string> Parse(string StringToCheck)
        {
            m_IdentifiedDutyType = DutyTypes.UNKNOWN;
            StringToCheck = StringToCheck.Replace(" ", "");
            StringToCheck = StringToCheck.Replace(@"\", "");
            StringToCheck = StringToCheck.Replace(@"/", "");
            StringToCheck = StringToCheck.Replace(@",", "");
            m_PotentialReferences=new List<string>();
            foreach (string Ref in ParseForPAYE(StringToCheck))
            {
                m_PotentialReferences.Add(Ref);
                m_IdentifiedDutyType = DutyTypes.PAYE;
            }
            foreach(string Ref in ParseForCOTAX(StringToCheck))
            {
                m_PotentialReferences.Add(Ref);
                m_IdentifiedDutyType = DutyTypes.COTAX;
            }
            foreach(string Ref in ParseForSA(StringToCheck))
            {
                m_PotentialReferences.Add(Ref);
                m_IdentifiedDutyType = DutyTypes.SA;
            }
            foreach (string Ref in ParseForSAFE(StringToCheck))
            {
                m_PotentialReferences.Add(Ref);
                m_IdentifiedDutyType = DutyTypes.SAFE;
            }
            //string temp = ParseForNINO(StringToCheck);
            //if(string.IsNullOrEmpty(temp) == false)
            //{
            //    m_PotentialReferences.Add(temp);
            //    m_IdentifiedDutyType = DutyTypes.NINO;
            //}

            //No longer permitted
            //foreach (string Ref in ParseForCT(StringToCheck))
            //{
            //    m_PotentialReferences.Add(Ref);
            //    m_IdentifiedDutyType = DutyTypes.CT;
            //}
            //No longer permitted
            //foreach (string Ref in ParseForITCP(StringToCheck))
            //{
            //    m_PotentialReferences.Add(Ref);
            //    m_IdentifiedDutyType = DutyTypes.ITCP;
            //}
            foreach(string Ref in ParseForNTC(StringToCheck))
            {
                m_PotentialReferences.Add(Ref);
                m_IdentifiedDutyType = DutyTypes.NTC;
            }
            foreach (string Ref in ParseForSDLT(StringToCheck))
            {
                m_PotentialReferences.Add(Ref);
                m_IdentifiedDutyType = DutyTypes.SDLT;
            }
            //TODO Awaiting confirmation of MOS reference structures
            //foreach (string Ref in ParseForMoss(StringToCheck))
            //{
            //    m_PotentialReferences.Add(Ref);
            //    m_IdentifiedDutyType = DutyTypes.MOSS;
            //}
            if (m_PotentialReferences.Count > 1)
            {
                m_IdentifiedDutyType = DutyTypes.UNKNOWN;
            }
            return m_PotentialReferences;
        }

        /// <summary>
        /// Checks whether a string contains a COTAX reference
        /// </summary>
        /// <param name="COTAXText">The string to be checked</param>
        /// <returns>A list containing any valid COTAX references</returns>
        public List<string> ParseForCOTAX(string COTAXText)
        {
            List<string> COTAXRefs = new List<string>();
            ReferenceCheck.CotaxReferenceChecker CheckCOTAX = new ReferenceCheck.CotaxReferenceChecker();
            string TempString = COTAXText.Replace(" ", "");
            while (TempString.Length >= 14)
            {
                if (m_GeneralTools.checkNumeric(TempString.Substring(0, 10)) == true)
                {
                    string ValidRef = CheckCOTAX.checkReference(TempString.Substring(0, 14));
                    if (ValidRef != "")
                    {
                        if(COTAXRefs.IndexOf(ValidRef) == -1)
                        {
                            COTAXRefs.Add(ValidRef);
                        }
                    }    
                }
                TempString = TempString.Remove(0, 1);
            }
            return COTAXRefs;
        }

        /// <summary>
        /// Checks whether a string contains a PAYE reference.
        /// </summary>
        /// <param name="PAYEText">The string to be checked</param>
        /// <returns>A list containing any valid PAYE references</returns>
        public List<string> ParseForPAYE(string PAYEText)
        {
            ReferenceCheck.PAYERReference CheckPAYE = new ReferenceCheck.PAYERReference();
            List<string> PAYERefs = new List<string>();
            string TempString = PAYEText.Replace(" ","");
            string BuiltReference;
            int Position = 0;
            while (TempString.Length >= 6)
            {
                //Check if the letter P appears in the string
                Position = TempString.IndexOf("P");
                if (Position == -1)
                {
                    return PAYERefs;
                }
                //Check whether there are 3 digits prior to the P
                if (Position < 3)
                {
                    //Can't be a PAYE ref - look at this code when loop added
                    TempString = TempString.Remove(0, 1);
                }
                else
                {
                    TempString = TempString.Substring(Position - 3, TempString.Length - (Position - 3));
                    if (TempString.Length >= 6)
                    {
                        BuiltReference = TempString.Substring(0, 4);
                        //Check if there is a letter after the P
                        if (m_GeneralTools.checkNumeric(TempString.Substring(4, 1)) == false)
                        {
                            BuiltReference = BuiltReference + TempString.Substring(4, 1);
                            Position = 4;
                            //Check that there are numbers after the second letter
                            string Serial = ExtractSerialNo(TempString.Substring(5, TempString.Length - 5), 8);
                            TempString = TempString.Substring(5 + Serial.Length, TempString.Length - (Serial.Length + 5));
                            if (TempString.Length != 0 && Serial.Length != 8 && TempString.Substring(0, 1).ToUpper() == "X")
                            {
                                if (Serial != "")
                                {
                                    Serial = Serial.PadLeft(7, '0') + "X";
                                    TempString = TempString.Remove(0, 1);
                                }
                            }
                            else
                            {
                                if (Serial != "")
                                {
                                    Serial = Serial.PadLeft(8, '0');
                                }
                            }
                            BuiltReference = (BuiltReference + Serial).ToUpper();
                            //Test the reference
                            string ValidRef = CheckPAYE.checkReference(BuiltReference);
                            if (ValidRef != "")
                            {
                                if (PAYERefs.IndexOf(ValidRef) == -1)
                                {
                                    //If valid add to the list
                                    PAYERefs.Add(ValidRef);
                                }
                            }
                        }
                        else
                        {
                            //Doesn't have a check character - manual check or calculate one?
                            TempString = TempString.Remove(0, 1);
                        }
                    }
                }
            }
            return PAYERefs;
        }

        /// <summary>
        /// Checks whether a string contains an SA reference
        /// </summary>
        /// <param name="SAText">The string to be checked</param>
        /// <returns>A list containing any valid SA references</returns>
        public List<string> ParseForSA(string SAText)
        {
            List<string> SARefs = new List<string>();
            ReferenceCheck.SAReferenceChecker CheckSA = new ReferenceCheck.SAReferenceChecker();
            string TempString = SAText.Replace(" ", "");
            int Position = TempString.ToUpper().IndexOf('K');
            while (TempString.Length >= 11 && Position != -1)
            {
                if (Position >= 10 && m_GeneralTools.checkNumeric(TempString.Substring(Position-10, 10)) == true)
                {
                    string ValidRef = CheckSA.checkReference(TempString.Substring(Position - 10, 11));
                    if (ValidRef != "")
                    {
                        if(SARefs.IndexOf(ValidRef) == -1)
                        {
                        SARefs.Add(ValidRef);
                        }
                    }
                }
                TempString = TempString.Remove(0, (Position+1));
                Position = TempString.ToUpper().IndexOf('K');
            }
            return SARefs;
        }

        /// <summary>
        /// Extracts SAFE references from a specified string. The methods will identify SAFE charge references
        /// and SAFE customer account references
        /// </summary>
        /// <param name="SAFEText">The string to be checked for possible references</param>
        /// <returns>A list of the SAFE references found in the string</returns>
        public List<string> ParseForSAFE(string SAFEText)
        {
            List<string> SAFERefs = new List<string>();
            string TempString = SAFEText;
            int Position = TempString.ToUpper().IndexOf('X');
            while(Position != -1 && TempString.Substring(Position, TempString.Length - Position).Length >= 14)
            {
                string ValidRef = "";
                ReferenceCheck.SAFEChargeReferenceChecker CheckCharge = new ReferenceCheck.SAFEChargeReferenceChecker();
                ValidRef = CheckCharge.checkReference(TempString.Substring(Position, 14));
                if (ValidRef != "")
                {
                    if (SAFERefs.IndexOf(ValidRef) == -1)
                    {
                        SAFERefs.Add(ValidRef);
                    }
                }
                if (TempString.Substring(Position, TempString.Length - Position).Length >= 15)
                {
                    ReferenceCheck.SAFECARReferenceChecker CheckCAR = new ReferenceCheck.SAFECARReferenceChecker();
                    ValidRef = CheckCAR.checkReference(TempString.Substring(Position, 15));
                    if (ValidRef != "")
                    {
                        if (SAFERefs.IndexOf(ValidRef) == -1)
                        {
                            SAFERefs.Add(ValidRef);
                        }
                    }
                }
                TempString = TempString.Substring(Position+1,TempString.Length - (Position+1));
                Position = TempString.ToUpper().IndexOf('X');
            }
            return SAFERefs;
        }

        /// <summary>
        /// TODO Placeholder code that only finds a match if the whole text is a NINO - code will need to be included to
        /// //step through the text in 9 character chunks but I do not want this to affect CHAPS validation
        /// </summary>
        /// <param name="NINOText"></param>
        /// <returns></returns>
        public string ParseForNINO(string NINOText)
        {
            ReferenceCheck.NINOReference CheckNINO = new ReferenceCheck.NINOReference();
            string validRef = CheckNINO.CheckNINO(NINOText);
            return validRef;
        }

        /// <summary>
        /// Extracts CT references from a specified string.
        /// </summary>
        /// <param name="CTText">The string to be checked for references</param>
        /// <returns>A list of the CT references found within the string</returns>
        public List<string> ParseForCT(string CTText)
        {
            List<string> CTRefs = new List<string>();
            string TempString = CTText;
            int Position = TempString.ToUpper().IndexOf('C');
            while (Position != -1 && TempString.Substring(Position, TempString.Length - Position).Length >= 11)
            {
                string ValidRef = "";
                ReferenceCheck.CTReferenceChecker CheckCT = new ReferenceCheck.CTReferenceChecker();
                ValidRef = CheckCT.checkReference(TempString.Substring(Position, 11));
                if (ValidRef != "")
                {
                    if (CTRefs.IndexOf(ValidRef) == -1)
                    {
                        CTRefs.Add(ValidRef);
                    }
                }
                TempString = TempString.Substring(Position + 1, TempString.Length - (Position + 1));
                Position = TempString.ToUpper().IndexOf('x');
            }

            return CTRefs;
        }

        /// <summary>
        /// Extracts ITCP references from a string
        /// </summary>
        /// <param name="ITCPText">The string to be checked for references</param>
        /// <returns>A list of the IT-CP references in the string</returns>
        public List<string> ParseForITCP(string ITCPText)
        {
            List<string> ITCPRefs = new List<string>();
            string TempString = ITCPText;
            int Position = TempString.ToUpper().IndexOf('F');
            while (Position != -1 && TempString.Substring(Position, TempString.Length - Position).Length >= 11)
            {
                string ValidRef = "";
                ReferenceCheck.ITCPReference CheckITCP = new ReferenceCheck.ITCPReference();
                ValidRef = CheckITCP.checkReference(TempString.Substring(Position, 11));
                if (ValidRef != "")
                {
                    if (ITCPRefs.IndexOf(ValidRef) == -1)
                    {
                        ITCPRefs.Add(ValidRef);
                    }
                }
                TempString = TempString.Substring(Position + 1, TempString.Length - (Position + 1));
                Position = TempString.ToUpper().IndexOf('x');
            }

            return ITCPRefs;
        }

        /// <summary>
        /// Extracts the NTC references from a string
        /// </summary>
        /// <param name="NTCText">The text to be parsed</param>
        /// <returns>A list of the NTC references in the string</returns>
        public List<string> ParseForNTC(string NTCText)
        {
            List<string> NTCRefs = new List<string>();
            string TempString = NTCText;
            ReferenceCheck.NTCRReference CheckNTC = new ReferenceCheck.NTCRReference();
            string ValidRef = "";
            while (TempString.Length >= 16)
            {
                ValidRef = CheckNTC.CheckReferenceIncludingDate(TempString.Substring(0, 16));
                if (ValidRef != "")
                {
                    NTCRefs.Add(ValidRef);
                }
                TempString = TempString.Remove(0, 1);
            }
            return NTCRefs;
        }

        /// <summary>
        /// Extracts the SDLT references from a string
        /// </summary>
        /// <param name="SDLTText">The txt to be parsed</param>
        /// <returns>A list of SDLT refrences in the string</returns>
        public List<string> ParseForSDLT(string SDLTText)
        {
            List<string> SDLTRefs = new List<string>();
            string TempString = SDLTText;
            ReferenceCheck.SDLTReferenceChecker CheckSDLT = new ReferenceCheck.SDLTReferenceChecker();
            string ValidRef = "";
            while (TempString.Length >= 11)
            {
                ValidRef = CheckSDLT.checkReference(TempString.Substring(0, 11));
                if (ValidRef != "")
                {
                    SDLTRefs.Add(ValidRef);
                }
                TempString = TempString.Remove(0, 1);
            }
            return SDLTRefs;
        }

        /// <summary>
        /// Checks a string for potential 15 and 17 character moss references
        /// </summary>
        /// <param name="MossText">The string that contains the reference</param>
        /// <returns>A list of the valid references within the string</returns>
        /// <remarks>This method is not currently part of the method that searches a string for any valid reference for any duty type
        /// VAT Moss is currently only supported by one system (EPT unallocated) and should not be valid in any other</remarks>
        public List<string> ParseForGBMoss(string MossText)
        {
            MossText = MossText.ToUpper();
            List<string> MOSSRefs = new List<string>();
            while (MossText.Length>=15)
            {
                if (MossText.Length >= 17)
                {
                    Regex objEndCharacters = new Regex(@"[GBEU][0-9]{9}[/][0-9]{3}");
                    if (objEndCharacters.IsMatch(MossText.Substring(0,17)) == true)
                    {
                        ReferenceCheck.MOSS17ReferenceChecker myMossCheck = new ReferenceCheck.MOSS17ReferenceChecker();
                        string result = myMossCheck.checkReference(MossText.Substring(0, 17));
                        if (string.IsNullOrEmpty(result) == false) MOSSRefs.Add(result);
                    }
                }
                else
                {
                    if (MossText.Length >= 15)
                    {
                        ReferenceCheck.MOSS15ReferenceChecker myMossCheck = new ReferenceCheck.MOSS15ReferenceChecker();
                        string result = myMossCheck.checkReference(MossText.Substring(0, 15));
                        if (string.IsNullOrEmpty(result) == false) MOSSRefs.Add(result);
                    }
                }
                MossText = MossText.Remove(0, 1);
            }
            return MOSSRefs;
        }

        /// <summary>
        /// Extracts a possible serial number from a string and then returns it to the calling code provided
        /// it can be converted to an integer
        /// </summary>
        /// <param name="PassedString">The string to be checked</param>
        /// <param name="MaxLength">The number of characters</param>
        /// <returns>A string containing the extracted serial number. A blank string means that it was not
        /// possible to extract a serial number</returns>
        private string ExtractSerialNo(string PassedString,int MaxLength)
        {
            string LocalString = "";
            if (PassedString != "")
            {
                foreach (Char character in PassedString)
                {
                    if(m_GeneralTools.checkNumeric(character.ToString()) == true)
                    {
                        LocalString = LocalString + character;
                        if (LocalString.Length == MaxLength)
                        {
                            return LocalString;
                        }
                    }
                    else
                    {
                        return LocalString;
                    }
                }
            }
            return LocalString;
        }

    }
}
