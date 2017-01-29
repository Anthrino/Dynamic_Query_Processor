using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Dynamic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Linq.Dynamic;
using Microsoft.CSharp;

namespace DynQueryProcessor
{
    
    class Program
    {
        static CSharpCodeProvider cdp = new CSharpCodeProvider();
        static CodeCompileUnit ccu = new CodeCompileUnit();
        static CompilerParameters cparams = new CompilerParameters();
        static CodeEntryPointMethod start = new CodeEntryPointMethod();
        static CodeNamespace ns = new CodeNamespace("Internal");
        static string filepath = @"F:\Jerin'\Jerin\Codetest5";
        
        static void Main( string[] args )
        {

            #region Initialisation //Section 
            CodeTypeDeclaration m_class = new CodeTypeDeclaration("Program");
            
            ccu.ReferencedAssemblies.Add("System.dll");
            ccu.ReferencedAssemblies.Add("System.Core.dll");
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            ns.Imports.Add(new CodeNamespaceImport("System.Linq"));
            ccu.Namespaces.Add(ns);

            Console.ForegroundColor = ConsoleColor.Green;

            XDocument xd = XDocument.Load(@"F:\Jerin'\Jerin\Xmltest.xml");
            XElement xe1 = XElement.Load(@"F:\Jerin'\Jerin\Xmltest.xml");

            string queryfile = @"F:\Jerin'\Jerin\QueryTest.txt";
            string query = QueryReader(queryfile);
            //string modq = QueryIsolator(query);
            Console.Write(" Enter name of field you wish to query (Student/Employee/Department) : ");
            List<dynamic> datalist = XmlListGen(xd , Console.ReadLine());

            start.Comments.Add(new CodeCommentStatement("methodregion"));
            m_class.Members.Add(start);
            ns.Types.Add(m_class);

            #endregion
            
            #region Experiment //Section (CodeDOM)
            //Var_Creater("a",typeof(System.Int32));
            //Var_Creater("b",typeof(System.Int32));
            //Var_Creater("sum",typeof(System.Int32));

            //CodeObjectCreateExpression temp = new CodeObjectCreateExpression(l_class);
            //foreach (var item in cvrs.Where( varref => varref.VariableName == "a" || varref.VariableName == "b"))
            //{
            //     temp.Parameters.Add( item );
            //}
            //cocs.Add( temp );
            //cmis.Add(new CodeMethodInvokeExpression( new CodeMethodReferenceExpression( new CodePrimitiveExpression(" ")));
            //cas.Add(new CodeAssignStatement(cvrs.Where( e => e.VariableName == "a").Single(),
            //    new CodePrimitiveExpression(5)));
            //cas.Add(new CodeAssignStatement(cvrs.Where( e => e.VariableName == "b").Single(),
            //    new CodePrimitiveExpression(10)));
            //cas.Add(new CodeAssignStatement(cvrs.Where( e => e.VariableName == "sum").Single(),
            //    new CodeBinaryOperatorExpression(cvrs.Where( e => e.VariableName == "a")
            //        .Single() ,CodeBinaryOperatorType.Add, cvrs.Where( e => e.VariableName == "b").Single())));
            //cmis.Add(new CodeMethodInvokeExpression( new CodeTypeReferenceExpression
            //    ( "System.Console" ) , "WriteLine" , new CodePrimitiveExpression( "Sum = " + cvrs.Where( e => e.VariableName == "sum" ) ) )); 
            #endregion

            #region Experiment //Section (LinQ)
            //var elist = xe1.Elements();
            //var resultset = datalist.Where(dept => dept.DName.Contains("Computer")).Select(dept => dept.Name);

            //XmlWriter xw1 = xe1.CreateWriter();
            //XmlReader xr1 = xe1.CreateReader();
            //xr1.ReadStartElement();
            //String s1 = xr1.ReadOuterXml();
            //Console.WriteLine(s1);
            //XmlElement x1 = xd.DocumentElement;


            //var slist = elist.Where().Select( file => file );
            //var slist1 = elist.Descendants().Where( e => e.Value.Contains( sval ) ).Select( e => e );

            //foreach( var item in resultset )
            //{
            //    Console.WriteLine( item );
            //} 
            #endregion

            
            
            cparams.ReferencedAssemblies.Add( "System.dll" );
            cparams.ReferencedAssemblies.Add( "System.Core.dll" );
            cparams.ReferencedAssemblies.Add( "Microsoft.CSharp.dll");
            cparams.ReferencedAssemblies.Add( "System.Data.dll");
            cparams.ReferencedAssemblies.Add( "System.Xml.dll");
            cparams.ReferencedAssemblies.Add( "System.Xml.Linq.dll" );
            cparams.GenerateExecutable = false;
            cparams.GenerateInMemory = true;
            cparams.TreatWarningsAsErrors = false;
            cparams.CompilerOptions = "/optimize";

            string codefile = CodeFileGenerator(ccu);
            string methodcode = "\n\t public List<dynamic> QueryFire(List<dynamic> data) \n\t{" +
                               "\n\t\t try {" +
                                       "\n\t\t\t var result = " +query+".Cast<dynamic>().ToList();" +
                                        "\n\t\t\t return result ;" +
                                   "\n\t\t }\n\t\t catch(Exception ex) " +
                                   "\n\t\t { \n\t\t\t return new List<dynamic>{ex.Message + ex.StackTrace}; \n\t\t } \n\t } \n\n";

            File.WriteAllText(codefile, System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(codefile), "// methodregion", methodcode));

            using (StreamReader sr = new StreamReader(filepath))
            {
                while (!sr.EndOfStream)
                    Console.WriteLine(sr.ReadLine());
            }
            Console.WriteLine("-------------------------------------------------------------------------------- \n\n\t Query Output :- \n");
            #region Execution // Section

            var result = cdp.CompileAssemblyFromFile(cparams, codefile);

            if (result.Errors.Count > 0)
            {
                foreach (CompilerError ce in result.Errors)
                {
                    Console.WriteLine(" Error : " + ce.ErrorText + " on line " + ce.Line);
                }
            }
            else
            {
                dynamic instance = Activator.CreateInstance(result.CompiledAssembly.GetType("Internal.Program"));

                dynamic output = instance.QueryFire(datalist);
               
                foreach (var item in output)
                {
                    Console.WriteLine(item);
                }
              
            }
            #endregion

            Console.Read();
        }
        static string QueryReader( string Filename )
        {
            String query;
            using( StreamReader sr = new StreamReader( Filename ) )
            {
                query = sr.ReadLine();
            }
            return query;
        }
        static List<dynamic> XmlListGen(XDocument xd , string searchfield)
        {
            List<dynamic> datalist = new List<dynamic>();
            if (searchfield == "Employee")
            {
                var elist = xd.Descendants("Employee").Select(s => s);
                foreach (var item in elist)
                {
                    dynamic emp = new ExpandoObject();
                    emp.Ename = item.Element("Ename").Value;
                    emp.Salary = item.Element("Salary").Value;
                    emp.DName = item.Element("DName").Value;
                    emp.EmpID = item.Element("EmpID").Value;
                    datalist.Add(emp);
                }
                foreach (var item in elist)
                {
                    Console.WriteLine(item.Name + item.Value);
                }
            }
            else if (searchfield == "Student")
            {
                var elist = xd.Descendants("Student").Select(s => s);
                foreach (var item in elist)
                {
                    dynamic student = new ExpandoObject();
                    student.Name = item.Element("Name").Value;
                    student.Ssn = item.Element("Ssn").Value;
                    student.DName = item.Element("DName").Value;

                    datalist.Add(student);
                }
                foreach (var item in elist)
                {
                    Console.WriteLine(item.Name + item.Value);
                }
            }
            else
            {
                var elist = xd.Descendants("Department").Select(s => s);
                foreach (var item in elist)
                {
                    dynamic dept = new ExpandoObject();
                    dept.Intake = item.Element("Intake").Value;
                    dept.DName = item.Element("DName").Value;
                    datalist.Add(dept);
                }
                foreach (var item in elist)
                {
                    Console.WriteLine(item.Name + item.Value);
                }

            }
            return datalist;

        }
        static string QueryIsolator(string query)
        {
            Dictionary<string , string> qsegments = new Dictionary<string , string>();

            string nminus = query.Substring( query.IndexOf( '.' ) );
            string descendantq = null;
            qsegments.Add( "nminus" , nminus);
            string listname = query.Remove(query.IndexOf(qsegments.Where( q => q.Key == "nminus" ).Select( q => q.Value).Single()));
            qsegments.Add( "listname" , listname );
            if (query.Contains("Descendants"))
            {
                descendantq = nminus.Substring(nminus.IndexOf("Descendants"));
                descendantq = descendantq.Remove(descendantq.IndexOf(")"));
                descendantq = descendantq.Replace("Descendants(", "");
                qsegments.Add("desc", descendantq);
            }
            if ( query.Contains( "Where" ))
            {
                string whereq = nminus.Substring( nminus.IndexOf( "Where" ) );
                whereq = whereq.Remove( whereq.IndexOf( ")" ) );
                whereq = whereq.Replace( "Where(" , "" );
                qsegments.Add( "where" , whereq ); 
            }
            if( query.Contains( "Select" ) )
            {
                string selectq = nminus.Substring( nminus.IndexOf( "Select" ) );
                selectq = selectq.Remove( selectq.IndexOf( ")" ) );
                selectq = selectq.Replace( "Select(" , "" );
                qsegments.Add( "select" , selectq );
            }

            //foreach( var item in qsegments )
            //{
            //    Console.WriteLine( item );
            //}
            //string[] wherequery = query.Split( new string[] { "Where(" , ")" } , StringSplitOptions.None );

            //Expression body = System.Linq.Dynamic.DynamicExpression.Parse( null , query , elist );
            //LambdaExpression e = Expression.Lambda( body );
            //Console.WriteLine( e.ToString() );

            //qsegments.Add( where , wherequery );
            Console.WriteLine(nminus);
            return descendantq;
           
        }
        static string CodeFileGenerator(CodeCompileUnit ccu)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            if (provider.FileExtension[0] != '.')
            {
                filepath += ".";
            }
            filepath += provider.FileExtension;
            using (StreamWriter sw = new StreamWriter(filepath, false))
            {
                IndentedTextWriter itw = new IndentedTextWriter(sw);
                provider.GenerateCodeFromCompileUnit(ccu, itw, new CodeGeneratorOptions());
            }
            
            return filepath;

        }
    }
}