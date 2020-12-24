using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Xml2Sql
{
    class Program
    {
        static void Main(string[] args)
        {
            GetFeildSet();

            Console.WriteLine("Amount = {0}", tpgs.Count);

            Console.WriteLine("-----\npress enter to wirte to sql.");

            GenSql();

            Console.Read();
            Console.Read();
        }

        private static void GetFeildSet()
        {
            const string inputFile = @"D:\Desktop\NQL_Database\PatentIsuRegSpecXMLA_047034\index.xml";
            XmlNodeReader reader = null;

            const string tag_area_top = "tw-patent-grant";

            int idx = 0;

            TPG tpg = new TPG();
            FieldInfo[] fieldInfos = typeof(TPG).GetFields(BindingFlags.Public | BindingFlags.Instance);
            int infoIdx = 0;
            string currentTag = "";
            string currentValue = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(inputFile);
                reader = new XmlNodeReader(doc);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == tag_area_top)
                        {
                            Console.WriteLine("Start - {0}", ++idx);
                            tpg = new TPG();
                            tpg.tw_patent_grant.certificate_number = reader.GetAttribute("certificate-number");
                            infoIdx++;
                        }
                        else if (reader.Name == "application-reference")
                        {
                            tpg.application_reference.appl_type = reader.GetAttribute("appl-type");
                        }
                        else if (reader.Name == "priority-claims")
                        {
                            startPriorityClaims = true;
                        }
                        else if (reader.Name == "priority-claim")
                        {
                            tpg.ClearTemp();
                            tpg.tempPC.sequence = reader.GetAttribute("sequence");
                        }
                        else if (reader.Name == "main-classification")
                        {
                            tpg.classification_ipc.main_classification.edition = reader.GetAttribute("edition");
                        }
                        else if (reader.Name == "further-classification")
                        {
                            tpg.classification_ipc.further_classification.edition = reader.GetAttribute("edition");
                        }
                        else if (reader.Name == "chinese-name")
                        {
                            startChineseName = true;
                        }
                        else if (reader.Name == "english-name")
                        {
                            startEnglishName = true;
                        }
                        else if (reader.Name == "applicants")
                        {
                            startApplicants = true;
                        }
                        else if (reader.Name == "applicant")
                        {
                            tpg.ClearTemp();
                            tpg.tempAP.sequence = reader.GetAttribute("sequence");
                            tpg.tempAP.app_type = reader.GetAttribute("app-type");
                        }
                        else if (reader.Name == "inventors")
                        {
                            startInventors = true;
                        }
                        else if (reader.Name == "inventor")
                        {
                            tpg.ClearTemp();
                            tpg.tempIN.sequence = reader.GetAttribute("sequence");
                            tpg.tempIN.app_type = reader.GetAttribute("app-type");
                        }
                        else if (reader.Name == "agents")
                        {
                            startAgents = true;
                        }
                        else if (reader.Name == "agent")
                        {
                            tpg.ClearTemp();
                            tpg.tempAG.sequence = reader.GetAttribute("sequence");
                            tpg.tempAG.rep_type = reader.GetAttribute("rep-type");
                        }
                        else if (reader.Name == "claims")
                        {
                            startClaims = true;
                        }
                        else if (reader.Name == "claim")
                        {
                            tpg.ClearTemp();
                            tpg.tempCL.id = reader.GetAttribute("id");
                            tpg.tempCL.num = reader.GetAttribute("num");
                        }
                        currentTag = reader.Name;
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        currentValue = reader.Value;
                        string f = fieldInfos[infoIdx].Name.Replace('_', '-').ToLower();
                        if (currentTag == f)
                        {
                            fieldInfos[infoIdx].SetValue(tpg, currentValue);

                            infoIdx++;
                        }
                        else if (currentTag == "doc-number" && (f == "publication-reference" || f == "certificate-number" || f == "application-reference"))
                        {
                            switch (f)
                            {
                                case "publication-reference":
                                    tpg.publication_reference.doc_number = currentValue;
                                    break;
                                case "certificate-number":
                                    tpg.certificate_number.doc_number = currentValue;
                                    break;
                                case "application-reference":
                                    tpg.application_reference.doc_number = currentValue;
                                    break;
                            }
                            infoIdx++;
                        }
                        else if (f == "invention-title")
                        {
                            if (currentTag == "chinese-title")
                            {
                                tpg.invention_title.chinese_title = currentValue;
                            }
                            else if (currentTag == "english-title")
                            {
                                tpg.invention_title.english_title = currentValue;
                                infoIdx++;
                            }
                        }
                        else if (startPriorityClaims)
                        {
                            if (currentTag == "country")
                            {
                                tpg.tempPC.country = currentValue;
                            }
                            else if (currentTag == "doc-number")
                            {
                                tpg.tempPC.doc_number = currentValue;
                            }
                            else if (currentTag == "date")
                            {
                                tpg.tempPC.date = currentValue;
                            }
                        }
                        else if (currentTag == "main-classification")
                        {
                            tpg.classification_ipc.main_classification.value = currentValue;
                        }
                        else if (currentTag == "further-classification")
                        {
                            tpg.classification_ipc.further_classification.value = currentValue;
                        }
                        else if (startApplicants)
                        {
                            if (startChineseName)
                            {
                                if (currentTag == "last-name")
                                {
                                    tpg.tempAP.addressbook.chinese_name.last_name = currentValue;
                                }
                                else if (currentTag == "first-name")
                                {
                                    tpg.tempAP.addressbook.chinese_name.first_name = currentValue;
                                }
                            }
                            else if (startEnglishName)
                            {
                                if (currentTag == "last-name")
                                {
                                    tpg.tempAP.addressbook.english_name.last_name = currentValue;
                                }
                                else if (currentTag == "middle-name")
                                {
                                    tpg.tempAP.addressbook.english_name.middle_name = currentValue;
                                }
                                else if (currentTag == "first-name")
                                {
                                    tpg.tempAP.addressbook.english_name.first_name = currentValue;
                                }
                            }
                            else if (currentTag == "address")
                            {
                                tpg.tempAP.addressbook.address = currentValue;
                            }
                            else if (currentTag == "english-country")
                            {
                                tpg.tempAP.addressbook.english_country = currentValue;
                            }
                        }
                        else if (startInventors)
                        {
                            if (startChineseName)
                            {
                                if (currentTag == "last-name")
                                {
                                    tpg.tempIN.addressbook.chinese_name.last_name = currentValue;
                                }
                                else if (currentTag == "first-name")
                                {
                                    tpg.tempIN.addressbook.chinese_name.first_name = currentValue;
                                }
                            }
                            else if (startEnglishName)
                            {
                                if (currentTag == "last-name")
                                {
                                    tpg.tempIN.addressbook.english_name.last_name = currentValue;
                                }
                                else if (currentTag == "middle-name")
                                {
                                    tpg.tempIN.addressbook.english_name.middle_name = currentValue;
                                }
                                else if (currentTag == "first-name")
                                {
                                    tpg.tempIN.addressbook.english_name.first_name = currentValue;
                                }
                            }
                            else if (currentTag == "address")
                            {
                                tpg.tempIN.addressbook.address = currentValue;
                            }
                            else if (currentTag == "english-country")
                            {
                                tpg.tempIN.addressbook.english_country = currentValue;
                            }
                        }
                        else if (startAgents)
                        {
                            if (startChineseName)
                            {
                                if (currentTag == "last-name")
                                {
                                    tpg.tempAG.addressbook.chinese_name.last_name = currentValue;
                                }
                                else if (currentTag == "first-name")
                                {
                                    tpg.tempAG.addressbook.chinese_name.first_name = currentValue;
                                }
                            }
                            else if (startEnglishName)
                            {
                                if (currentTag == "last-name")
                                {
                                    tpg.tempAG.addressbook.english_name.last_name = currentValue;
                                }
                                else if (currentTag == "middle-name")
                                {
                                    tpg.tempAG.addressbook.english_name.middle_name = currentValue;
                                }
                                else if (currentTag == "first-name")
                                {
                                    tpg.tempAG.addressbook.english_name.first_name = currentValue;
                                }
                            }
                            else if (currentTag == "address")
                            {
                                tpg.tempAG.addressbook.address = currentValue;
                            }
                            else if (currentTag == "english-country")
                            {
                                tpg.tempAG.addressbook.english_country = currentValue;
                            }
                        }
                        else if (startClaims)
                        {
                            if (currentTag == "p")
                            {
                                tpg.tempCL.claim = currentValue;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == tag_area_top)
                        {
                            tpgs.Add(tpg);
                            tpg.ClearTemp();
                            infoIdx = 0;
                        }
                        else if (reader.Name == "priority-claims")
                        {
                            startPriorityClaims = false;
                            infoIdx++;
                        }
                        else if (reader.Name == "priority-claim")
                        {
                            tpg.priority_claims.Add(tpg.tempPC);
                        }
                        else if (reader.Name == "chinese-name")
                        {
                            startChineseName = false;
                        }
                        else if (reader.Name == "english-name")
                        {
                            startEnglishName = false;
                        }
                        else if (reader.Name == "applicants")
                        {
                            startApplicants = false;
                        }
                        else if (reader.Name == "applicant")
                        {
                            tpg.parties.applicants.Add(tpg.tempAP);
                        }
                        else if (reader.Name == "inventors")
                        {
                            startInventors = false;
                        }
                        else if (reader.Name == "inventor")
                        {
                            tpg.parties.inventors.Add(tpg.tempIN);
                        }
                        else if (reader.Name == "agents")
                        {
                            startAgents = false;
                        }
                        else if (reader.Name == "agent")
                        {
                            tpg.parties.agents.Add(tpg.tempAG);
                        }
                        else if (reader.Name == "claims")
                        {
                            startClaims = false;
                        }
                        else if (reader.Name == "claim")
                        {
                            tpg.claims.Add(tpg.tempCL);
                        }
                    }
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                tpg.ClearTemp();
            }
        }

        private static string Conv(string s)
        {
            if (s == null)
                return "\"\"";

            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\'", "\\\'") + "\"";
        }

        private static void GenSql()
        {
            string header = "use tpg;";

            /*Dictionary<string, string[]> tables = new Dictionary<string, string[]>()
            {
                { "tw_bibliographic_data_grant", new string[] { "certificate_number", "volno", "isuno" } },
                { "application_reference",       new string[] { "certificate_number", "appl_type", "doc_number" }  },
                { "invention_title",             new string[] { "certificate_number", "chinese_title", "english_title" } },
                { "priority_claim",              new string[] { "certificate_number", "sequence", "country", "doc_number", "priority_claim_date" } },
                { "classification_ipc",          new string[] { "certificate_number", "main_classification", "main_classification_edition", "further_classification", "further_classification_edition" } },
                { "inventor",                    new string[] { "certificate_number", "sequence", "app_type", "chinese_name_last_name", "chinese_name_first_name", "english_name_last_name", "english_name_middle_name", "english_name_first_name", "address", "english_country" } },
                { "applicant",                   new string[] { "certificate_number", "sequence", "app_type", "chinese_name_last_name", "chinese_name_first_name", "english_name_last_name", "english_name_middle_name", "english_name_first_name", "address", "english_country" } },
                { "agent",                       new string[] { "certificate_number", "sequence", "rep_type", "chinese_name_last_name", "chinese_name_first_name", "english_name_last_name", "english_name_middle_name", "english_name_first_name", "address", "english_country" } },
                { "claim",                       new string[] { "certificate_number", "id", "content" } }
            };*/
            /*
            Dictionary<string, string[]> tables = new Dictionary<string, string[]>()
            {
                { "獲准資料",     new string[] { "證書編號", "卷數", "期別" } },
                { "申請案號資料", new string[] { "證書編號", "申請類型", "申請文件編號" }  },
                { "發明標題",     new string[] { "證書編號", "中文標題", "英文標題" } },
                { "優先權",       new string[] { "證書編號", "優先權排序", "優先權國家", "優先權文件編號", "優先權日期" } },
                { "國際專利分類", new string[] { "證書編號", "主分類號", "主分類號版本", "其他分類號", "其他分類號版本" } },
                { "發明者",      new string[] { "證書編號", "發明者排序", "發明者類型", "發明者中文姓氏", "發明者中文名字", "發明者英文姓氏", "發明者英文中間名", "發明者英文名字", "發明者地址", "發明者英文國家" } },
                { "申請人",      new string[] { "證書編號", "申請人排序", "申請人類型", "申請人中文姓氏", "申請人中文名字", "申請人英文姓氏", "申請人英文中間名", "申請人英文名字", "申請人地址", "申請人英文國家" } },
                { "代理人",      new string[] { "證書編號", "代理人排序", "代理人類型", "代理人中文姓氏", "代理人中文名字", "代理人英文姓氏", "代理人英文中間名", "代理人英文名字", "代理人地址", "代理人英文國家" } },
                { "申請權利範圍", new string[] { "證書編號", "申請權利範圍編號", "申請權利範圍內容" } }
            };
            */
            Dictionary<string, string[]> tables = new Dictionary<string, string[]>()
            {
                { "獲准資料",     new string[] { "證書編號", "卷數", "期別" } },
                { "申請案號資料", new string[] { "證書編號", "申請類型", "申請文件編號" }  },
                { "發明標題",     new string[] { "證書編號", "中文標題", "英文標題" } },
                { "優先權",       new string[] { "證書編號", "優先權排序", "優先權國家", "優先權文件編號", "優先權日期" } },
                { "國際專利分類", new string[] { "證書編號", "主分類號", "主分類號版本", "其他分類號", "其他分類號版本" } },
                { "發明者",      new string[] { "證書編號", "發明者排序", "發明者類型", "發明者中文名稱", "發明者英文名稱", "發明者英文國家" } },
                { "申請者",      new string[] { "證書編號", "申請者排序", "申請者類型", "申請者中文名稱", "申請者英文名稱", "申請者地址", "申請者英文國家" } },
                { "代理者",      new string[] { "證書編號", "代理者排序", "代理者類型", "代理者中文名稱", "代理者地址" } },
                { "申請權利範圍", new string[] { "證書編號", "申請權利範圍編號", "申請權利範圍內容" } }
            };

            List<string> values = new List<string>();

            Console.WriteLine("Generating");

            foreach (TPG tpg in tpgs)
            {
                foreach (string table in tables.Keys)
                {
                    if (table == "獲准資料")
                    {
                        values.Add(Conv(tpg.certificate_number.doc_number));

                        values.Add(tpg.volno);
                        values.Add(tpg.isuno);

                        header += GetInsertCommand(table, tables[table], values);
                        values.Clear();
                    }
                    else if (table == "申請案號資料")
                    {
                        values.Add(Conv(tpg.certificate_number.doc_number));

                        values.Add(Conv(tpg.application_reference.appl_type));
                        values.Add(Conv(tpg.application_reference.doc_number));

                        header += GetInsertCommand(table, tables[table], values);
                        values.Clear();
                    }
                    else if (table == "發明標題")
                    {
                        values.Add(Conv(tpg.certificate_number.doc_number));

                        values.Add(Conv(tpg.invention_title.chinese_title));
                        values.Add(Conv(tpg.invention_title.english_title));

                        header += GetInsertCommand(table, tables[table], values);
                        values.Clear();
                    }
                    else if (table == "優先權")
                    {
                        foreach (var pc in tpg.priority_claims)
                        {
                            values.Add(Conv(tpg.certificate_number.doc_number));

                            values.Add(Conv(pc.sequence));
                            values.Add(Conv(pc.country));
                            values.Add(Conv(pc.doc_number));
                            if (pc.date != "")
                                values.Add(Conv(DateTime.ParseExact(pc.date, "yyyyMMdd", null).ToString("yyyy-MM-dd")));
                            else
                                values.Add("null");

                            header += GetInsertCommand(table, tables[table], values);
                            values.Clear();
                        }
                    }
                    else if (table == "國際專利分類")
                    {
                        values.Add(Conv(tpg.certificate_number.doc_number));

                        values.Add(Conv(tpg.classification_ipc.main_classification.value));
                        values.Add(Conv(tpg.classification_ipc.main_classification.edition));
                        values.Add(Conv(tpg.classification_ipc.further_classification.value));
                        values.Add(Conv(tpg.classification_ipc.further_classification.edition));

                        header += GetInsertCommand(table, tables[table], values);
                        values.Clear();
                    }
                    else if (table == "發明者")
                    {
                        foreach (var inventor in tpg.parties.inventors)
                        {
                            values.Add(Conv(tpg.certificate_number.doc_number));

                            values.Add(inventor.sequence);
                            values.Add(Conv(inventor.app_type));
                            values.Add(Conv(inventor.addressbook.chinese_name.last_name));
                            // values.Add(Conv(inventor.addressbook.chinese_name.first_name));
                            values.Add(Conv(inventor.addressbook.english_name.last_name));
                            // values.Add(Conv(inventor.addressbook.english_name.middle_name));
                            // values.Add(Conv(inventor.addressbook.english_name.first_name));
                            // values.Add(Conv(inventor.addressbook.address));
                            values.Add(Conv(inventor.addressbook.english_country));

                            header += GetInsertCommand(table, tables[table], values);
                            values.Clear();
                        }
                    }
                    else if (table == "申請者")
                    {
                        foreach (var applicant in tpg.parties.applicants)
                        {
                            values.Add(Conv(tpg.certificate_number.doc_number));

                            values.Add(applicant.sequence);
                            values.Add(Conv(applicant.app_type));
                            values.Add(Conv(applicant.addressbook.chinese_name.last_name));
                            // values.Add(Conv(applicant.addressbook.chinese_name.first_name));
                            values.Add(Conv(applicant.addressbook.english_name.last_name));
                            // values.Add(Conv(applicant.addressbook.english_name.middle_name));
                            // values.Add(Conv(applicant.addressbook.english_name.first_name));
                            values.Add(Conv(applicant.addressbook.address));
                            values.Add(Conv(applicant.addressbook.english_country));

                            header += GetInsertCommand(table, tables[table], values);
                            values.Clear();
                        }
                    }
                    else if (table == "代理者")
                    {
                        foreach (var agent in tpg.parties.agents)
                        {
                            values.Add(Conv(tpg.certificate_number.doc_number));

                            values.Add(agent.sequence);
                            values.Add(Conv(agent.rep_type));
                            values.Add(Conv(agent.addressbook.chinese_name.last_name));
                            // values.Add(Conv(agent.addressbook.chinese_name.first_name));
                            // values.Add(Conv(agent.addressbook.english_name.last_name));
                            // values.Add(Conv(agent.addressbook.english_name.middle_name));
                            // values.Add(Conv(agent.addressbook.english_name.first_name));
                            values.Add(Conv(agent.addressbook.address));
                            // values.Add(Conv(agent.addressbook.english_country));

                            header += GetInsertCommand(table, tables[table], values);
                            values.Clear();
                        }
                    }
                    else if (table == "申請權利範圍")
                    {
                        foreach (var claim in tpg.claims)
                        {
                            values.Add(Conv(tpg.certificate_number.doc_number));

                            values.Add(claim.num);
                            values.Add(Conv(claim.claim));

                            header += GetInsertCommand(table, tables[table], values);
                            values.Clear();
                        }
                    }
                }
            }

            Console.WriteLine("Writing");

            const string outputFile = @"D:\Desktop\NQL_Database\output_sql_command.sql";
            using (StreamWriter sw = File.CreateText(outputFile))
                sw.WriteLine(header);

            Console.WriteLine("Done");

        }

        private static string GetInsertCommand(string table, string[] fieldNames, List<string> values)
        {
            string fs = string.Join(",", fieldNames);
            string vs = string.Join(",", values);
            return string.Format("insert into {0}({1})values({2});", table, fs, vs);
        }

        private static List<TPG> tpgs = new List<TPG>();

        private static bool startPriorityClaims = false;
        private static bool startChineseName = false;
        private static bool startEnglishName = false;
        private static bool startApplicants = false;
        private static bool startInventors = false;
        private static bool startAgents = false;
        private static bool startClaims = false;

        private class TPG
        {
            public TPG()
            {
                tw_patent_grant.certificate_number = "";
                volno = "";
                isuno = "";
                publication_reference.doc_number = "";
                certificate_number.doc_number = "";
                application_reference.appl_type = "";
                application_reference.doc_number = "";
                invention_title.chinese_title = "";
                invention_title.english_title = "";
                classification_ipc.main_classification.edition = "";
                classification_ipc.main_classification.value = "";
                classification_ipc.further_classification.edition = "";
                classification_ipc.further_classification.value = "";
                ClearTemp();
                parties.applicants = new List<Parties.Applicant>();
                parties.inventors = new List<Parties.Inventor>();
                parties.agents = new List<Parties.Agent>();
            }
            public void ClearTemp()
            {
                tempPC.sequence = "";
                tempPC.country = "";
                tempPC.doc_number = "";
                tempPC.date = "";

                tempAP.sequence = "";
                tempAP.app_type = "";

                tempAP.addressbook.chinese_name.last_name = "";
                tempAP.addressbook.chinese_name.first_name = "";
                tempAP.addressbook.chinese_name.name_type = "";

                tempAP.addressbook.english_name.last_name = "";
                tempAP.addressbook.english_name.middle_name = "";
                tempAP.addressbook.english_name.first_name = "";
                tempAP.addressbook.english_name.name_type = "";

                tempAP.addressbook.address = "";
                tempAP.addressbook.english_country = "";

                tempCL.claim = "";
            }
            public Tw_patent_grant tw_patent_grant;
            public string volno;
            public string isuno;
            public Publication_reference publication_reference;
            public Certificate_number certificate_number;
            public Application_reference application_reference;
            public Inventio_title invention_title;
            public List<Priority_claim> priority_claims = new List<Priority_claim>();
            public Classification_ipc classification_ipc;
            public Parties parties;

            public Priority_claim tempPC = new Priority_claim();
            public Parties.Applicant tempAP = new Parties.Applicant();
            public Parties.Inventor tempIN = new Parties.Inventor();
            public Parties.Agent tempAG = new Parties.Agent();
            public Claim tempCL = new Claim();

            public struct Tw_patent_grant
            {
                public string certificate_number;
            }
            public struct Publication_reference
            {
                public string doc_number;
            }
            public struct Certificate_number
            {
                public string doc_number;
            }
            public struct Application_reference
            {
                public string appl_type;
                public string doc_number;
            }
            public struct Inventio_title
            {
                public string chinese_title;
                public string english_title;
            }
            public struct Priority_claim
            {
                public string sequence;
                public string country;
                public string doc_number;
                public string date;
            }
            public struct Classification_ipc
            {
                public Main_classification main_classification;
                public Further_classification further_classification;

                public struct Main_classification
                {
                    public string edition;
                    public string value;
                }
                public struct Further_classification
                {
                    public string edition;
                    public string value;
                }
            }
            public struct Parties
            {
                public List<Applicant> applicants;
                public List<Inventor> inventors;
                public List<Agent> agents;

                public struct Applicant
                {
                    public string sequence;
                    public string app_type;
                    public Addressbook addressbook;
                }
                public struct Inventor
                {
                    public string sequence;
                    public string app_type;
                    public Addressbook addressbook;
                }
                public struct Agent
                {
                    public string sequence;
                    public string rep_type;
                    public Addressbook addressbook;
                }
                public struct Addressbook
                {
                    public Chinese_name chinese_name;
                    public English_name english_name;
                    public string address;
                    public string english_country;

                    public struct Chinese_name
                    {
                        public string last_name;
                        public string first_name;
                        public string name_type;
                    }
                    public struct English_name
                    {
                        public string last_name;
                        public string middle_name;
                        public string first_name;
                        public string name_type;
                    }
                }
            }
            public struct Claim
            {
                public string id;
                public string num;
                public string claim;
            }

            public List<Claim> claims = new List<Claim>();
        }
    }
}
