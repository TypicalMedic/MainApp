using PluginInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace MainApp
{
    public partial class Form1 : Form
    {
        Dictionary<string, IPlugin> plugins = new Dictionary<string, IPlugin>();
        public Form1()
        {
            InitializeComponent();
            FindPlugins();
            CreatePluginsMenu();
        }

        void CreatePluginsMenu()
        {
            foreach (IPlugin p in plugins.Values)
            {
                var menuItem = new ToolStripMenuItem(p.Name);
                menuItem.Click += OnPluginClick;

                filtersToolStripMenuItem.DropDownItems.Add(menuItem);
            }

        }

        void FindPlugins()
        {
            // папка с плагинами
            string folder = System.AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(folder).Parent.Parent.FullName;
            string[] files;
            if (File.Exists(projectDirectory + "\\App.config"))
            {
                foreach (string x in ConfigurationManager.AppSettings)
                {
                    files = Directory.GetFiles(ConfigurationManager.AppSettings.Get(x), "*.dll");
                    foreach (string file in files)
                        try
                        {
                            Assembly assembly = Assembly.LoadFile(file);

                            foreach (Type type in assembly.GetTypes())
                            {
                                Type iface = type.GetInterface("PluginInterface.IPlugin");

                                if (iface != null)
                                {
                                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                                    plugins.Add(plugin.Name, plugin);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка загрузки плагина\n" + ex.Message);
                        }
                }
            }
            else
            {// dll-файлы в этой папке
                files = Directory.GetFiles(folder, "*.dll");

                foreach (string file in files)
                    try
                    {
                        Assembly assembly = Assembly.LoadFile(file);

                        foreach (Type type in assembly.GetTypes())
                        {
                            Type iface = type.GetInterface("PluginInterface.IPlugin");

                            if (iface != null)
                            {
                                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                                plugins.Add(plugin.Name, plugin);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка загрузки плагина\n" + ex.Message);
                    }
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void OnPluginClick(object sender, EventArgs args)
        {
            IPlugin plugin = plugins[((ToolStripMenuItem)sender).Text];
            plugin.Transform((Bitmap)pictureBox.Image);
            pictureBox.Refresh();
        }

        private void plugindInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string res = "Подключенные плагины\n\n";
            foreach (var x in plugins)
            {
                res += "Имя: " + x.Value.Name + "\nАвтор: " + x.Value.Author;
                res += "\nВерсия: " + GetAttribute(x.Value.GetType()) + "\n\n";
            }
            MessageBox.Show(res);
        }
        public static string GetAttribute(Type t)
        {
            VersionAttribute MyAttribute =
                (VersionAttribute)Attribute.GetCustomAttribute(t, typeof(VersionAttribute));

            if (MyAttribute == null)
            {
                return "The attribute was not found.";
            }
            else
            {
                return MyAttribute.Major + "." + MyAttribute.Minor;
            }
        }
    }
}
