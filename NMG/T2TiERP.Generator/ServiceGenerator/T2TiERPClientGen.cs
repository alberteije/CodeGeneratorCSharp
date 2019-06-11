using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;
using System.IO;
using System.Reflection;
using NMG.Core;
using System.Xml.XPath;
using T2TiERP.Generator.Properties;
using System.Xml;

namespace T2TiERP.Generator.ServiceGenerator
{
    /// <summary>
    /// 
    /// </summary>

    public class T2TiERPClientGen
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
        public string nomeTabelaPai { get; set; }
        public string tipoDTO { get; set; }
        private ApplicationPreferences appPref{get;set;}

        public T2TiERPClientGen(string filePath, string specificFolder, string tableName, string nameSpace, string assemblyName, string sequenceName, Table table, ApplicationPreferences appPrefs)
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
            appPref = appPrefs;
        }

        private string substituirMarcacoesArquivo(string entrada)
        {
            try
            {
                entrada = entrada.Replace("[TipoDTO]", tipoDTO);
                entrada = entrada.Replace("[NomeTabela]", nomeTabela);
                entrada = entrada.Replace("[NomeTabelaPai]", nomeTabelaPai);
                entrada = entrada.Replace("[NamespaceCliente]", appPref.NamespaceCliente);
                entrada = entrada.Replace("[AssemblyCliente]", appPref.AssemblyCliente);

                return entrada;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewModel()
        {
            try
            {
                using (TextReader viewModelTxt = new StringReader(Resources.DTOViewModel))
                {
                    string conteudoArquivo = viewModelTxt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarCodigoAuxiliar(string nomeTabPai)
        {
            try
            {
                this.nomeTabelaPai = nomeTabPai;

                using (TextReader txt = new StringReader(Resources.CodigoAuxiliar))
                {
                    string conteudoArquivo = txt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public string gerarViewDTOListaXAML()
        {
            try
            {
                using (TextReader viewModelTxt = new StringReader(Resources.DTOLista_xaml))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(viewModelTxt);

                    //Acessa o nó datagrid.columns
                    XmlNode grid = xmlDoc.FirstChild.LastChild.LastChild.LastChild.FirstChild;

                    foreach (Column coluna in Table.Columns)
                    {
                        XmlElement dataGridTextColumn = xmlDoc.CreateElement("DataGridTextColumn");
                        dataGridTextColumn.SetAttribute("Header", Formatter.FormatText(coluna.Name));
                        if (coluna.DataType.Length > 3 && coluna.DataType.Substring(0, 4).Contains("date"))
                        {
                            dataGridTextColumn.SetAttribute("Binding", "{Binding Path=" + Formatter.FormatText(coluna.Name) + ",StringFormat=dd/MM/yyyy}");
                        }
                        else if (coluna.DataType.Length > 6 && coluna.DataType.Substring(0, 7).Contains("decimal"))
                        {
                            dataGridTextColumn.SetAttribute("Binding", "{Binding Path=" + Formatter.FormatText(coluna.Name) + ",Converter={StaticResource moedaFormat}}");
                        }else
                            dataGridTextColumn.SetAttribute("Binding", "{Binding Path=" + Formatter.FormatText(coluna.Name) + "}");

                        grid.AppendChild(dataGridTextColumn);
                    }

                    string conteudoArquivo = "";
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                    using (var stringWriter = new StringWriter())
                    using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                    {
                        xmlDoc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        conteudoArquivo = stringWriter.GetStringBuilder().ToString();
                    }

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDTOListaCS()
        {
            try
            {
                using (TextReader viewModelTxt = new StringReader(Resources.DTOLista_xaml_cs))
                {
                    string conteudoArquivo = viewModelTxt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDTOXAML()
        {
            try
            {
                using (TextReader viewTxt = new StringReader(Resources.DTO_xaml))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(viewTxt);

                    //Acessa o nó Grid
                    XmlNode grid = xmlDoc.FirstChild.FirstChild.LastChild.LastChild;

                    int left = 6;
                    int top = 6;
                    int contLinha = 0;
                    foreach (Column coluna in Table.Columns)
                    {
                        contLinha++;
                        int larguraTextBox = 100;

                        if (coluna.IsForeignKey)
                        {
                            string nomeTabelaFK = Formatter.FormatText(coluna.ForeignKey.References);
                            XmlElement textbox = xmlDoc.CreateElement("TextBox");
                            textbox.SetAttribute("HorizontalAlignment", "Left");
                            textbox.SetAttribute("VerticalAlignment", "Top");
                            textbox.SetAttribute("IsReadOnly", "True");
                            textbox.SetAttribute("Text", "{Binding " + nomeTabela + "Selected." + nomeTabelaFK + ".Id}");
                            textbox.SetAttribute("Height", "23");
                            textbox.SetAttribute("Width", (larguraTextBox - 32).ToString());
                            textbox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");
                            grid.AppendChild(textbox);

                            XmlElement button = xmlDoc.CreateElement("Button");
                            button.SetAttribute("HorizontalAlignment", "Left");
                            button.SetAttribute("VerticalAlignment", "Top");
                            button.SetAttribute("Height", "23");
                            button.SetAttribute("Content", "...");
                            button.SetAttribute("Width", "30");
                            button.SetAttribute("Name", "btPesquisar" + nomeTabelaFK);
                            button.SetAttribute("Click", "btPesquisar" + nomeTabelaFK + "_Click");
                            button.SetAttribute("Margin", left + (larguraTextBox - 32) + "," + (top + 19) + ",0,0");
                            grid.AppendChild(button);
                        }else if (coluna.DataType.Substring(0,4).Contains("char"))
                        {
                            XmlElement combobox = xmlDoc.CreateElement("ComboBox");
                            combobox.SetAttribute("HorizontalAlignment", "Left");
                            combobox.SetAttribute("VerticalAlignment", "Top");
                            combobox.SetAttribute("Height", "23");
                            combobox.SetAttribute("Width", larguraTextBox.ToString());
                            combobox.SetAttribute("SelectedValue", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            combobox.SetAttribute("SelectedValuePath", "Tag");
                            combobox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");

                            XmlElement cbSim = xmlDoc.CreateElement("ComboBoxItem");
                            cbSim.SetAttribute("Content", "Sim");
                            cbSim.SetAttribute("Tag", "S");
                            combobox.AppendChild(cbSim);
                            XmlElement cbNao = xmlDoc.CreateElement("ComboBoxItem");
                            cbNao.SetAttribute("Content", "Não");
                            cbNao.SetAttribute("Tag", "N");
                            combobox.AppendChild(cbNao);

                            grid.AppendChild(combobox);
                        }
                        else if (coluna.DataType.Substring(0, 4).Contains("date"))
                        {
                            XmlElement datePicker = xmlDoc.CreateElement("DatePicker");
                            datePicker.SetAttribute("HorizontalAlignment", "Left");
                            datePicker.SetAttribute("VerticalAlignment", "Top");
                            datePicker.SetAttribute("Height", "23");
                            datePicker.SetAttribute("Width", larguraTextBox.ToString());
                            datePicker.SetAttribute("SelectedDate", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            datePicker.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");

                            grid.AppendChild(datePicker);
                        }
                        else if (coluna.DataType.Substring(0, 4).Contains("text"))
                        {
                            XmlElement textbox = xmlDoc.CreateElement("TextBox");
                            textbox.SetAttribute("HorizontalAlignment", "Left");
                            textbox.SetAttribute("VerticalAlignment", "Top");
                            textbox.SetAttribute("Text", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            textbox.SetAttribute("Height", "23");
                            textbox.SetAttribute("TextWrapping", "Wrap");
                            textbox.SetAttribute("VerticalContentAlignment", "Top");
                            textbox.SetAttribute("AcceptsReturn", "True");
                            textbox.SetAttribute("Width", larguraTextBox.ToString());
                            textbox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");
                            grid.AppendChild(textbox);
                        }
                        else
                        {
                            XmlElement textbox = xmlDoc.CreateElement("TextBox");
                            textbox.SetAttribute("HorizontalAlignment", "Left");
                            textbox.SetAttribute("VerticalAlignment", "Top");
                            textbox.SetAttribute("Text", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            textbox.SetAttribute("Height", "23");
                            textbox.SetAttribute("Width", larguraTextBox.ToString());
                            textbox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");
                            grid.AppendChild(textbox);
                        }

                        XmlElement label = xmlDoc.CreateElement("Label");
                        label.SetAttribute("Content", Formatter.FormatSingular(coluna.Name));
                        label.SetAttribute("HorizontalAlignment", "Left");
                        label.SetAttribute("VerticalAlignment", "Top");
                        label.SetAttribute("Margin", left + "," + top + ",0,0");
                        grid.AppendChild(label);

                        left += larguraTextBox + 6;

                        if (contLinha > 5)
                        {
                            contLinha = 0;
                            left = 6;
                            top += 48;
                        }

                    }

                    string conteudoArquivo = "";
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                    using (var stringWriter = new StringWriter())
                    using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter,settings))
                    {
                        xmlDoc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        conteudoArquivo= stringWriter.GetStringBuilder().ToString();
                    }

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDTOCS()
        {
            try
            {
                using (TextReader viewTxt = new StringReader(Resources.DTO_xaml_cs))
                {
                    string conteudoArquivo = viewTxt.ReadToEnd();

                    foreach (Column coluna in Table.Columns)
                    {
                        if (coluna.IsForeignKey)
                        {
                            using (TextReader eventosView = new StringReader(Resources.EventosView))
                            {
                                string conteudo = eventosView.ReadToEnd();
                                conteudo = conteudo.Replace("[NomeReferencia]", Formatter.FormatText(coluna.ForeignKey.References));
                                int pos = conteudoArquivo.IndexOf("[EventoPesquisar]");
                                conteudoArquivo = conteudoArquivo.Insert(pos+17, Environment.NewLine);
                                conteudoArquivo = conteudoArquivo.Insert(pos + 18, conteudo);
                            }
                        }
                    }

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDTOPrincipalXAML()
        {
            try
            {
                using (TextReader viewTxt = new StringReader(Resources.DTOPrincipal_xaml))
                {
                    string conteudoArquivo = viewTxt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDTOPrincipalCS()
        {
            try
            {
                using (TextReader viewTxt = new StringReader(Resources.DTOPrincipal_xaml_cs))
                {
                    string conteudoArquivo = viewTxt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDetalheDTOPrincipalXAML(string nomeTabPai)
        {
            try
            {
                this.nomeTabelaPai = nomeTabPai;

                using (TextReader viewTxt = new StringReader(Resources.DTODetalhePrincipal_xaml))
                {
                    string conteudoArquivo = viewTxt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public string gerarViewDetalheDTOPrincipalCS(string nomeTabPai)
        {
            try
            {
                this.nomeTabelaPai = nomeTabPai;

                using (TextReader viewTxt = new StringReader(Resources.DTODetalhePrincipal_xaml_cs))
                {
                    string conteudoArquivo = viewTxt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDetalheDTOListaXAML(string nomeTabPai)
        {
            try
            {
                nomeTabelaPai = nomeTabPai;
                using (TextReader viewModelTxt = new StringReader(Resources.DTODetalheLista_xaml))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(viewModelTxt);

                    //Acessa o nó datagrid.columns
                    XmlNode grid = xmlDoc.FirstChild.LastChild.FirstChild.LastChild.FirstChild;

                    foreach (Column coluna in Table.Columns)
                    {
                        XmlElement dataGridTextColumn = xmlDoc.CreateElement("DataGridTextColumn");
                        dataGridTextColumn.SetAttribute("Header", Formatter.FormatText(coluna.Name));
                        if (coluna.DataType.Length > 3 && coluna.DataType.Substring(0, 4).Contains("date"))
                        {
                            dataGridTextColumn.SetAttribute("Binding", "{Binding Path=" + Formatter.FormatText(coluna.Name) + ",StringFormat=dd/MM/yyyy}");
                        }
                        else if (coluna.DataType.Length > 6 && coluna.DataType.Substring(0, 7).Contains("decimal"))
                        {
                            dataGridTextColumn.SetAttribute("Binding", "{Binding Path=" + Formatter.FormatText(coluna.Name) + ",Converter={StaticResource moedaFormat}}");
                        }
                        else
                            dataGridTextColumn.SetAttribute("Binding", "{Binding Path=" + Formatter.FormatText(coluna.Name) + "}");

                        grid.AppendChild(dataGridTextColumn);
                    }

                    string conteudoArquivo = "";
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                    using (var stringWriter = new StringWriter())
                    using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                    {
                        xmlDoc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        conteudoArquivo = stringWriter.GetStringBuilder().ToString();
                    }

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDetalheDTOListaCS(string nomeTabPai)
        {
            try
            {
                nomeTabelaPai = nomeTabPai;
                using (TextReader viewModelTxt = new StringReader(Resources.DTODetalheLista_xaml_cs))
                {
                    string conteudoArquivo = viewModelTxt.ReadToEnd();

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDetalheDTOXAML(string nomeTabPai)
        {
            try
            {
                nomeTabelaPai = nomeTabPai;
                using (TextReader viewTxt = new StringReader(Resources.DTODetalhe_xaml))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(viewTxt);

                    //Acessa o nó Grid
                    XmlNode grid = xmlDoc.FirstChild.FirstChild.LastChild;

                    int left = 6;
                    int top = 6;
                    int contLinha = 0;
                    foreach (Column coluna in Table.Columns)
                    {
                        contLinha++;
                        int larguraTextBox = 100;

                        if (coluna.IsForeignKey)
                        {
                            string nomeTabelaFK = Formatter.FormatText(coluna.ForeignKey.References);
                            XmlElement textbox = xmlDoc.CreateElement("TextBox");
                            textbox.SetAttribute("HorizontalAlignment", "Left");
                            textbox.SetAttribute("VerticalAlignment", "Top");
                            textbox.SetAttribute("IsReadOnly", "True");
                            textbox.SetAttribute("Text", "{Binding " + nomeTabela + "Selected." + nomeTabelaFK + ".Id}");
                            textbox.SetAttribute("Height", "23");
                            textbox.SetAttribute("Width", (larguraTextBox - 32).ToString());
                            textbox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");
                            grid.AppendChild(textbox);

                            XmlElement button = xmlDoc.CreateElement("Button");
                            button.SetAttribute("HorizontalAlignment", "Left");
                            button.SetAttribute("VerticalAlignment", "Top");
                            button.SetAttribute("Height", "23");
                            button.SetAttribute("Content", "...");
                            button.SetAttribute("Width", "30");
                            button.SetAttribute("Name", "btPesquisar" + nomeTabelaFK);
                            button.SetAttribute("Click", "btPesquisar" + nomeTabelaFK + "_Click");
                            button.SetAttribute("Margin", left + (larguraTextBox - 32) + "," + (top + 19) + ",0,0");
                            grid.AppendChild(button);
                        }
                        else if (coluna.DataType.Substring(0, 4).Contains("char"))
                        {
                            XmlElement combobox = xmlDoc.CreateElement("ComboBox");
                            combobox.SetAttribute("HorizontalAlignment", "Left");
                            combobox.SetAttribute("VerticalAlignment", "Top");
                            combobox.SetAttribute("Height", "23");
                            combobox.SetAttribute("Width", larguraTextBox.ToString());
                            combobox.SetAttribute("SelectedValue", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            combobox.SetAttribute("SelectedValuePath", "Tag");
                            combobox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");

                            XmlElement cbSim = xmlDoc.CreateElement("ComboBoxItem");
                            cbSim.SetAttribute("Content", "Sim");
                            cbSim.SetAttribute("Tag", "S");
                            combobox.AppendChild(cbSim);
                            XmlElement cbNao = xmlDoc.CreateElement("ComboBoxItem");
                            cbNao.SetAttribute("Content", "Não");
                            cbNao.SetAttribute("Tag", "N");
                            combobox.AppendChild(cbNao);

                            grid.AppendChild(combobox);
                        }
                        else if (coluna.DataType.Substring(0, 4).Contains("date"))
                        {
                            XmlElement datePicker = xmlDoc.CreateElement("DatePicker");
                            datePicker.SetAttribute("HorizontalAlignment", "Left");
                            datePicker.SetAttribute("VerticalAlignment", "Top");
                            datePicker.SetAttribute("Height", "23");
                            datePicker.SetAttribute("Width", larguraTextBox.ToString());
                            datePicker.SetAttribute("SelectedDate", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            datePicker.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");
                        }
                        else if (coluna.DataType.Substring(0, 4).Contains("text"))
                        {
                            XmlElement textbox = xmlDoc.CreateElement("TextBox");
                            textbox.SetAttribute("HorizontalAlignment", "Left");
                            textbox.SetAttribute("VerticalAlignment", "Top");
                            textbox.SetAttribute("Text", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            textbox.SetAttribute("Height", "23");
                            textbox.SetAttribute("TextWrapping", "Wrap");
                            textbox.SetAttribute("VerticalContentAlignment", "Top");
                            textbox.SetAttribute("AcceptsReturn", "True");
                            textbox.SetAttribute("Width", larguraTextBox.ToString());
                            textbox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");
                            grid.AppendChild(textbox);
                        }

                        else
                        {
                            XmlElement textbox = xmlDoc.CreateElement("TextBox");
                            textbox.SetAttribute("HorizontalAlignment", "Left");
                            textbox.SetAttribute("VerticalAlignment", "Top");
                            textbox.SetAttribute("Text", "{Binding " + nomeTabela + "Selected." + Formatter.FormatText(coluna.Name) + "}");
                            textbox.SetAttribute("Height", "23");
                            textbox.SetAttribute("Width", larguraTextBox.ToString());
                            textbox.SetAttribute("Margin", left + "," + (top + 19) + ",0,0");
                            grid.AppendChild(textbox);
                        }

                        XmlElement label = xmlDoc.CreateElement("Label");
                        label.SetAttribute("Content", Formatter.FormatSingular(coluna.Name));
                        label.SetAttribute("HorizontalAlignment", "Left");
                        label.SetAttribute("VerticalAlignment", "Top");
                        label.SetAttribute("Margin", left + "," + top + ",0,0");
                        grid.AppendChild(label);

                        left += larguraTextBox + 6;

                        if (contLinha > 5)
                        {
                            contLinha = 0;
                            left = 6;
                            top += 48;
                        }

                    }

                    string conteudoArquivo = "";
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                    using (var stringWriter = new StringWriter())
                    using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                    {
                        xmlDoc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        conteudoArquivo = stringWriter.GetStringBuilder().ToString();
                    }

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string gerarViewDetalheDTOCS(string nomeTabPai)
        {
            try
            {
                nomeTabelaPai = nomeTabPai;

                using (TextReader viewTxt = new StringReader(Resources.DTODetalhe_xaml_cs))
                {
                    string conteudoArquivo = viewTxt.ReadToEnd();

                    foreach (Column coluna in Table.Columns)
                    {
                        if (coluna.IsForeignKey)
                        {
                            using (TextReader eventosView = new StringReader(Resources.EventosView))
                            {
                                string conteudo = eventosView.ReadToEnd();
                                conteudo = conteudo.Replace("[NomeReferencia]", Formatter.FormatText(coluna.ForeignKey.References));
                                int pos = conteudoArquivo.IndexOf("[EventoPesquisar]");
                                conteudoArquivo = conteudoArquivo.Insert(pos + 17, Environment.NewLine);
                                conteudoArquivo = conteudoArquivo.Insert(pos + 18, conteudo);
                            }
                        }
                    }

                    return this.substituirMarcacoesArquivo(conteudoArquivo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
