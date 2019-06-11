using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMG.Core;
using System.Xml;
using NMG.Core.Domain;
using NMG.Core.Generator;
using System.IO;

namespace T2TiERP.Generator.ServiceGenerator
{
    public class T2TiERPNHibernateGen : MappingGenerator
    {
        private T2TiERPServiceGen serviceGenerator { get; set; }
        private T2TiERPClientGen clientGenerator { get; set; }
        private string NamespaceCliente { get; set; }
        private string AssemblyCliente { get; set; }
        public T2TiERPNHibernateGen(ApplicationPreferences applicationPreferences, Table table)
            : base(applicationPreferences, table)
        {
            serviceGenerator = new T2TiERPServiceGen(applicationPreferences.FolderPath, "Mapping", applicationPreferences.TableName, applicationPreferences.NameSpace, applicationPreferences.AssemblyName, applicationPreferences.Sequence, table, applicationPreferences);
            clientGenerator = new T2TiERPClientGen(applicationPreferences.FolderPath, "Mapping", applicationPreferences.TableName, applicationPreferences.NameSpace, applicationPreferences.AssemblyName, applicationPreferences.Sequence, table, applicationPreferences);
            NamespaceCliente = applicationPreferences.NamespaceCliente;
            AssemblyCliente = applicationPreferences.AssemblyCliente;
        }

        public override void Generate()
        {
            #region NHibernate
            string fileName = filePath + Formatter.FormatSingular(tableName) + "DTO.hbm.xml";
            using (var stringWriter = new StringWriter())
            {
                XmlDocument xmldoc = CreateMappingDocument();
                xmldoc.Save(stringWriter);
                string generatedXML = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(fileName))
                {
                    writer.Write(generatedXML);
                    writer.Flush();
                }
            }
            #endregion

            Directory.CreateDirectory(filePath+@"\Servico");
            #region Interface
            string arquivoInterface = filePath+@"\Servico\" +"I"+ Formatter.FormatSingular(tableName) + ".txt";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(serviceGenerator.gerarInterfaceServicos());
                string arqInterfaceTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoInterface))
                {
                    writer.Write(arqInterfaceTemp);
                    writer.Flush();
                }
            }
             */
            gravarArquivo(serviceGenerator.gerarInterfaceServicos(), arquivoInterface);
            #endregion

            #region Servico
            string arquivoServico = filePath + @"\Servico\" + "Servico" + Formatter.FormatSingular(tableName) + ".txt";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(serviceGenerator.gerarImplementacaoServicos());
                string arqServTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoServico))
                {
                    writer.Write(arqServTemp);
                    writer.Flush();
                }
            }
             */
            gravarArquivo(serviceGenerator.gerarImplementacaoServicos(), arquivoServico);
            #endregion

            Directory.CreateDirectory(filePath+@"\ViewModel");
            #region ViewModel
            
            string arquivoViewModel = filePath +@"\ViewModel\" + Formatter.FormatSingular(tableName) + "ViewModel.cs";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarViewModel());
                string arqCliTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoViewModel))
                {
                    writer.Write(arqCliTemp);
                    writer.Flush();
                }
            }
             */
            gravarArquivo(clientGenerator.gerarViewModel(), arquivoViewModel);
            #endregion

            Directory.CreateDirectory(filePath + @"\CodigoAux");
            #region CodigoAuxiliar
            string arquivo = filePath + @"\CodigoAux\" + Formatter.FormatSingular(tableName) + "CodigoAuxiliar.cs";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarCodigoAuxiliar());
                string arqCliTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivo))
                {
                    writer.Write(arqCliTemp);
                    writer.Flush();
                }
            }
             */ 
            gravarArquivo(clientGenerator.gerarCodigoAuxiliar(Formatter.FormatSingular("NomeTabelaPai")), arquivo);
            #endregion

            Directory.CreateDirectory(filePath+@"\Views");

            #region ViewLista
            string arquivoViewLista = filePath + @"\Views\" + Formatter.FormatSingular(tableName) + "Lista.xaml";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarViewDTOListaXAML());
                string arqCliTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoViewLista))
                {
                    writer.Write(arqCliTemp);
                    writer.Flush();
                }
            }
             */ 
            gravarArquivo(clientGenerator.gerarViewDTOListaXAML(), arquivoViewLista);

            arquivoViewLista = filePath + @"\Views\" + Formatter.FormatSingular(tableName) + "Lista.xaml.cs";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarViewDTOListaCS());
                string arqCliTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoViewLista))
                {
                    writer.Write(arqCliTemp);
                    writer.Flush();
                }
            }
             */ 
            gravarArquivo(clientGenerator.gerarViewDTOListaCS(), arquivoViewLista);
            #endregion

            #region ViewDTO
            string arquivoViewDTO = filePath + @"\Views\" + Formatter.FormatSingular(tableName) + ".xaml";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarViewDTOXAML());
                string arqTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoViewDTO))
                {
                    writer.Write(arqTemp);
                    writer.Flush();
                }
            }
             */
            gravarArquivo(clientGenerator.gerarViewDTOXAML(), arquivoViewDTO);

            arquivoViewDTO = filePath + @"\Views\" + Formatter.FormatSingular(tableName) + ".xaml.cs";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarViewDTOCS());
                string arqCliTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoViewDTO))
                {
                    writer.Write(arqCliTemp);
                    writer.Flush();
                }
            }
             */ 
            gravarArquivo(clientGenerator.gerarViewDTOCS(), arquivoViewDTO);

            #endregion

            #region ViewPrincipal
            string arquivoViewPrincipalDTO = filePath + @"\Views\" + Formatter.FormatSingular(tableName) + "Principal.xaml";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarViewDTOPrincipalXAML());
                string arqTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoViewPrincipalDTO))
                {
                    writer.Write(arqTemp);
                    writer.Flush();
                }
            }
             */ 
            gravarArquivo(clientGenerator.gerarViewDTOPrincipalXAML(), arquivoViewPrincipalDTO);

            arquivoViewPrincipalDTO = filePath + @"\Views\" + Formatter.FormatSingular(tableName) + "Principal.xaml.cs";
            /*
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(clientGenerator.gerarViewDTOPrincipalCS());
                string arqTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(arquivoViewPrincipalDTO))
                {
                    writer.Write(arqTemp);
                    writer.Flush();
                }
            }
             */ 
            gravarArquivo(clientGenerator.gerarViewDTOPrincipalCS(), arquivoViewPrincipalDTO);
            #endregion

            foreach (ForeignKey fk in Table.ForeignKeys)
            {
                Directory.CreateDirectory(filePath + @"\ViewsDetalhe");
                string arquivoDetalhe = filePath + @"\ViewsDetalhe\" + Formatter.FormatSingular(tableName) + "Principal.xaml";
                gravarArquivo(clientGenerator.gerarViewDetalheDTOPrincipalXAML(Formatter.FormatSingular(fk.References)), arquivoDetalhe);
                arquivoDetalhe = filePath + @"\ViewsDetalhe\" + Formatter.FormatSingular(tableName) + "Principal.xaml.cs";
                gravarArquivo(clientGenerator.gerarViewDetalheDTOPrincipalCS(Formatter.FormatSingular(fk.References)), arquivoDetalhe);
                arquivoDetalhe = filePath + @"\ViewsDetalhe\" + Formatter.FormatSingular(tableName) + "Lista.xaml";
                gravarArquivo(clientGenerator.gerarViewDetalheDTOListaXAML(Formatter.FormatSingular(fk.References)), arquivoDetalhe);
                arquivoDetalhe = filePath + @"\ViewsDetalhe\" + Formatter.FormatSingular(tableName) + "Lista.xaml.cs";
                gravarArquivo(clientGenerator.gerarViewDetalheDTOListaCS(Formatter.FormatSingular(fk.References)), arquivoDetalhe);
                arquivoDetalhe = filePath + @"\ViewsDetalhe\" + Formatter.FormatSingular(tableName) + ".xaml";
                gravarArquivo(clientGenerator.gerarViewDetalheDTOXAML(Formatter.FormatSingular(fk.References)), arquivoDetalhe);
                arquivoDetalhe = filePath + @"\ViewsDetalhe\" + Formatter.FormatSingular(tableName) + ".xaml.cs";
                gravarArquivo(clientGenerator.gerarViewDetalheDTOCS(Formatter.FormatSingular(fk.References)), arquivoDetalhe);

            }
        }

        private void gravarArquivo(string arquivo, string path)
        {
            using (var stringWriter = new StringWriter())
            {
                stringWriter.Write(arquivo);
                string arqTemp = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(path))
                {
                    writer.Write(arqTemp);
                    writer.Flush();
                }
            }
        }
        private static string RemoveEmptyNamespaces(string mappingContent)
        {
            mappingContent = mappingContent.Replace("utf-16", "utf-8");
            return mappingContent.Replace("xmlns=\"\"", "");
        }
        public override XmlDocument CreateMappingDocument()
        {
            var xmldoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmldoc.CreateXmlDeclaration("1.0", string.Empty, string.Empty);
            xmldoc.AppendChild(xmlDeclaration);
            XmlElement root = xmldoc.CreateElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
            root.SetAttribute("assembly", assemblyName);
            root.SetAttribute("namespace", nameSpace);
            root.SetAttribute("default-lazy", "false");
            xmldoc.AppendChild(root);

            XmlElement classElement = xmldoc.CreateElement("class");
            classElement.SetAttribute("name", Formatter.FormatSingular(tableName)+"DTO");
            classElement.SetAttribute("table", tableName);
            //classElement.SetAttribute("lazy", "true");
            root.AppendChild(classElement);
            PrimaryKey primaryKey = Table.PrimaryKey;


            if (primaryKey.Type == PrimaryKeyType.CompositeKey)
            {
                XmlElement idElement = xmldoc.CreateElement("composite-id");
                foreach (Column key in primaryKey.Columns)
                {
                    XmlElement keyProperty = xmldoc.CreateElement("key-property");
                    keyProperty.SetAttribute("name", Formatter.FormatText(key.Name));
                    keyProperty.SetAttribute("column", key.Name);

                    idElement.AppendChild(keyProperty);

                    classElement.AppendChild(idElement);
                }
            }


            foreach (Column column in Table.Columns)
            {
                XmlElement property = null;
                //XmlElement property2;

                if (column.IsForeignKey)
                {
                    property = xmldoc.CreateElement("many-to-one");
                    //property.SetAttribute("insert", "false");
                    //property.SetAttribute("update", "false");
                    //property.SetAttribute("lazy", "false");
                    //property2 = xmldoc.CreateElement("property");
                }
                else if (column.IsPrimaryKey)
                {
                    //property2 = xmldoc.CreateElement("id");
                    property = xmldoc.CreateElement("id");
                    XmlElement generatorElement = xmldoc.CreateElement("generator");
                    generatorElement.SetAttribute("class", "identity");
                    property.AppendChild(generatorElement);
                    //property2.AppendChild(generatorElement);
                }
                else
                {
                    property = xmldoc.CreateElement("property");
                }


                //if (property != null)
                property.SetAttribute("name", Formatter.FormatText(column.Name));
                property.SetAttribute("column", column.Name);
                if (column.IsForeignKey)
                {
                    property.SetAttribute("name", Formatter.FormatText(column.ForeignKey.References));
                }
                    

                //property2.SetAttribute("name", Formatter.FormatText(column.Name));
                //XmlElement columnProperty = xmldoc.CreateElement("column");
                //if (property != null)
                //    property.AppendChild(columnProperty);

                //columnProperty.SetAttribute("name", column.Name);
                //columnProperty.SetAttribute("sql-type", column.DataType);
                //columnProperty.SetAttribute("not-null", (!column.IsNullable).ToString().ToLower());
                //property2.AppendChild(columnProperty.Clone());
                //if (property != null)
                    classElement.AppendChild(property);
                //classElement.AppendChild(property2);
            }

            foreach (var hasMany in Table.HasManyRelationships)
            {
                XmlElement bagElement = xmldoc.CreateElement("bag");

                bagElement.SetAttribute("name", Formatter.FormatPlural(hasMany.Reference));
                bagElement.SetAttribute("inverse", "false");
                bagElement.SetAttribute("cascade", "all-delete-orphan");

                classElement.AppendChild(bagElement);

                XmlElement keyElement = xmldoc.CreateElement("key");

                keyElement.SetAttribute("column", hasMany.ReferenceColumn);

                bagElement.AppendChild(keyElement);

                XmlElement oneToManyElement = xmldoc.CreateElement("one-to-many");

                oneToManyElement.SetAttribute("class", Formatter.FormatSingular(hasMany.Reference)+"DTO");

                bagElement.AppendChild(oneToManyElement);
            }

            return xmldoc;
        }
        protected override void AddIdGenerator(XmlDocument xmldoc, XmlElement idElement)
        {
            var generatorElement = xmldoc.CreateElement("generator");
            generatorElement.SetAttribute("class", "identity");
            idElement.AppendChild(generatorElement);
        }
    }
}
