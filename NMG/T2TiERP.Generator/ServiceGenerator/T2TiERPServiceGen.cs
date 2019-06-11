using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using System.IO;
using NMG.Core.TextFormatter;
using NMG.Core.Generator;
using NMG.Core;

namespace T2TiERP.Generator.ServiceGenerator
{
    public class T2TiERPServiceGen 
    {
        protected Table Table;
        protected string assemblyName;
        protected string filePath;
        protected string nameSpace;
        protected string sequenceName;
        protected string tableName;
		internal const string TABS = "\t\t\t";
    	protected string ClassNamePrefix { get; set;}
        public ITextFormatter Formatter { get; set; }
        public string nomeTabela { get; set; }
        public string tipoDTO { get; set; }

        public T2TiERPServiceGen(string filePath, string specificFolder, string tableName, string nameSpace, string assemblyName, string sequenceName, Table table, ApplicationPreferences appPrefs)
        {
            this.filePath = filePath;
            if(appPrefs.GenerateInFolders)
            {
                this.filePath = Path.Combine(filePath, specificFolder);
                if(!this.filePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    this.filePath = this.filePath + Path.DirectorySeparatorChar;
                }
            }
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequenceName = sequenceName;
            Table = table;
            Formatter = TextFormatterFactory.GetTextFormatter(appPrefs);
            nomeTabela = Formatter.FormatSingular(tableName);
            tipoDTO = nomeTabela + "DTO";
        }

        public string gerarInterfaceServicos()
        {
            try
            {
                StringBuilder arquivo = new StringBuilder();
                /**
                 * Gera Métodos Interface Servico WCF
                 **/

                arquivo.AppendLine("#region " + nomeTabela);
                arquivo.AppendLine("[OperationContract]");
                arquivo.AppendLine("void Delete"+nomeTabela+"("+tipoDTO+" "+nomeTabela.ToCamelCase()+");");
                arquivo.AppendLine("[OperationContract]");
                arquivo.AppendLine(tipoDTO+" SalvarAtualizar" + nomeTabela + "(" + tipoDTO + " " + nomeTabela.ToCamelCase() + ");");
                arquivo.AppendLine("[OperationContract]");
                arquivo.AppendLine("IList<" + tipoDTO + "> Select" + nomeTabela + "(" + tipoDTO + " " + nomeTabela.ToCamelCase() + ");");
                arquivo.AppendLine("[OperationContract]");
                arquivo.AppendLine("IList<" + tipoDTO + "> Select" + nomeTabela + "Pagina(int primeiroResultado, int quantidadeResultados, " + tipoDTO + " " + nomeTabela.ToCamelCase() + ");");
                arquivo.AppendLine("#endregion ");

                return arquivo.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarImplementacaoServicos()
        {
            try
            {
                StringBuilder arquivo = new StringBuilder();
                /**
                * Gera Métodos Classe Servico WCF
                **/
                arquivo.AppendLine("#region " + nomeTabela);
                arquivo.AppendLine(gerarMetodoDelete());
                arquivo.AppendLine(gerarMetodoSaveOrUpdate());
                arquivo.AppendLine(gerarMetodoSelect());
                arquivo.AppendLine(gerarMetodoSelectPagina());
                arquivo.AppendLine("#endregion ");
                return arquivo.ToString();
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
        private string gerarMetodoSelectPagina()
        {
            StringBuilder metodoSelect = new StringBuilder();

            metodoSelect.AppendLine("public IList<" + tipoDTO + "> Select" + nomeTabela + "Pagina(int primeiroResultado, int quantidadeResultados, " + tipoDTO + " " + nomeTabela.ToCamelCase() + ")");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("try");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("IList<" + tipoDTO + "> Resultado = null;");
            metodoSelect.AppendLine("using (ISession Session = NHibernateHelper.GetSessionFactory().OpenSession())");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("NHibernateDAL<" + tipoDTO + "> DAL = new NHibernateDAL<" + tipoDTO + ">(Session);");
            metodoSelect.AppendLine("Resultado =  DAL.SelectPagina<" + tipoDTO + ">(primeiroResultado, quantidadeResultados, " + nomeTabela.ToCamelCase() + ");");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("return Resultado;");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("catch (Exception ex)");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("throw new FaultException(ex.Message + (ex.InnerException != null ? \" \" + ex.InnerException.Message : \"\"));");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("");

            return metodoSelect.ToString();
        }
        private string gerarMetodoSelect()
        {
            StringBuilder metodoSelect = new StringBuilder();

            metodoSelect.AppendLine("public IList<" + tipoDTO + "> Select" + nomeTabela + "(" + tipoDTO + " " + nomeTabela.ToCamelCase() + ")");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("try");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("IList<" + tipoDTO + "> Resultado = null;");
            metodoSelect.AppendLine("using (ISession Session = NHibernateHelper.GetSessionFactory().OpenSession())");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("NHibernateDAL<" + tipoDTO + "> DAL = new NHibernateDAL<" + tipoDTO + ">(Session);");
            metodoSelect.AppendLine("Resultado =  DAL.Select(" + nomeTabela.ToCamelCase() + ");");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("return Resultado;");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("catch (Exception ex)");
            metodoSelect.AppendLine("{");
            metodoSelect.AppendLine("throw new FaultException(ex.Message + (ex.InnerException != null ? \" \" + ex.InnerException.Message : \"\"));");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("}");
            metodoSelect.AppendLine("");

            return metodoSelect.ToString();
        }
        private string gerarMetodoSaveOrUpdate()
        {
            StringBuilder metodoSaveUpdate = new StringBuilder();

            metodoSaveUpdate.AppendLine("public " + tipoDTO + " SalvarAtualizar" + nomeTabela + "(" + tipoDTO + " " + nomeTabela.ToCamelCase() + ")");
            metodoSaveUpdate.AppendLine("{");
            metodoSaveUpdate.AppendLine("try");
            metodoSaveUpdate.AppendLine("{");
            metodoSaveUpdate.AppendLine("using (ISession Session = NHibernateHelper.GetSessionFactory().OpenSession())");
            metodoSaveUpdate.AppendLine("{");
            metodoSaveUpdate.AppendLine("NHibernateDAL<" + tipoDTO + "> DAL = new NHibernateDAL<" + tipoDTO + ">(Session);");
            metodoSaveUpdate.AppendLine("DAL.SaveOrUpdate(" + nomeTabela.ToCamelCase() + ");");
            metodoSaveUpdate.AppendLine("Session.Flush();");
            metodoSaveUpdate.AppendLine("}");
            metodoSaveUpdate.AppendLine("return " + nomeTabela.ToCamelCase() + ";");
            metodoSaveUpdate.AppendLine("}");
            metodoSaveUpdate.AppendLine("catch (Exception ex)");
            metodoSaveUpdate.AppendLine("{");
            metodoSaveUpdate.AppendLine("throw new FaultException(ex.Message + (ex.InnerException != null ? \" \" + ex.InnerException.Message : \"\"));");
            metodoSaveUpdate.AppendLine("}");
            metodoSaveUpdate.AppendLine("}");
            metodoSaveUpdate.AppendLine("");

            return metodoSaveUpdate.ToString();
        }
        private string gerarMetodoDelete()
        {
            StringBuilder metodoDelete = new StringBuilder();
            metodoDelete.AppendLine("public void Delete"+nomeTabela+"("+tipoDTO+" "+nomeTabela.ToCamelCase()+")");
            metodoDelete.AppendLine("{");
                metodoDelete.AppendLine("try");
                metodoDelete.AppendLine("{");
                    metodoDelete.AppendLine("using (ISession Session = NHibernateHelper.GetSessionFactory().OpenSession())");
                    metodoDelete.AppendLine("{");
                        metodoDelete.AppendLine("NHibernateDAL<"+tipoDTO+"> DAL = new NHibernateDAL<"+tipoDTO+">(Session);");
                        metodoDelete.AppendLine("DAL.Delete("+nomeTabela.ToCamelCase()+");");
                        metodoDelete.AppendLine("Session.Flush();");
                    metodoDelete.AppendLine("}");
                metodoDelete.AppendLine("}");
                metodoDelete.AppendLine("catch (Exception ex)");
                metodoDelete.AppendLine("{");
                    metodoDelete.AppendLine("throw new FaultException(ex.Message + (ex.InnerException != null ? \" \" + ex.InnerException.Message : \"\"));");
                metodoDelete.AppendLine("}");
            metodoDelete.AppendLine("}");
            metodoDelete.AppendLine("");


            return metodoDelete.ToString();
        }

    }
}
