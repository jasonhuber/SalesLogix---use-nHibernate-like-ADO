using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//Sage usings
using Sage.Platform.Configuration;
using Sage.Platform.EntityBinding;
using Sage.Platform.Services;
using Sage.Platform.WebPortal;
using Sage.Platform.WebPortal.Services;
using Sage.Platform.WebPortal.SmartParts;
using Sage.Platform.WebPortal.UI;
using Sage.Platform.WebPortal.Workspaces;
using Sage.Platform.Application;
using Sage.Platform.Application.UI;
using Sage.Platform.Application.UI.Web;
using Sage.Platform.Orm;
using Sage.Platform.Orm.Entities;
using Sage.Platform.Data;
using Sage.Platform;
using Sage.Entity.Interfaces;
using Sage.Platform.DynamicMethod;
using System.Reflection;

namespace EntityModelFromDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var conStr = @"Provider=SLXOLEDB.1;Password=;User ID=admin;Initial Catalog=SALESLOGIX_EVAL;Data Source=localhost";
            Sage.Platform.Application.ApplicationContext.Initialize("Temp");
            Sage.Platform.Application.ApplicationContext.Current.Services.Add<IDataService>(new ConnectionStringDataService(conStr));
            var mgr = Sage.Platform.Application.ApplicationContext.Current.Services.Get<ConfigurationManager>();
            mgr.RegisterConfigurationType(
                new ReflectionConfigurationTypeInfo
                {
                    ConfigurationType = typeof(HibernateConfiguration),
                    ConfigurationSourceType = typeof(InMemoryConfigurationSource)
                });
            mgr.WriteConfiguration(
                new HibernateConfiguration
                {
                    Dialect = "NHibernate.Dialect.MsSql2005Dialect",
                    ConnectionProvider = "Sage.Platform.Data.DataServiceConnectionProvider, Sage.Platform",
                    ConnectionDriver = "NHibernate.Driver.OleDbDriver",
                    ProxyFactory = "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle",
                    MappingAssemblies =
                    {
                        //list your POCO assemblies here
                        "Sage.SalesLogix.Entities",
                        "Sage.SalesLogix.Security.Entities"
                    }
                });
            mgr.RegisterConfigurationType(
        new ReflectionConfigurationTypeInfo
        {
            ConfigurationType = typeof(DynamicMethodConfiguration),
            ConfigurationSourceType = typeof(InMemoryConfigurationSource)
        });
            var config = new DynamicMethodConfiguration();
            foreach (var type in Sage.Common.Utilities.Assembly.Load("Sage.SalesLogix.BusinessRules").GetTypes())
            {
                var parts = type.FullName.Split('.');
                if (parts.Length == 4 && parts[3] == "Rules")
                {
                    var entityName = parts[2];
                    foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Public | BindingFlags.Static))
                    {
                        config.Methods.Add(
                            new BusinessRuleMethod(string.Format("{0}.{1}", entityName, method.Name),
                                                   method.ReturnType == typeof(void) ? DynamicMethodReturnMode.None : DynamicMethodReturnMode.Object,
                                                   method.ReturnType.FullName)
                            {
                                PrimaryTarget = new MethodTargetElement(type.AssemblyQualifiedName, method.Name)
                            });
                    }
                }
            }
            mgr.WriteConfiguration(config);


            using (new SessionScopeWrapper())
            {

                IAccount newaccount = (IAccount)EntityFactory.GetRepository<IAccount>().Create();
                newaccount.AccountName = "Jason";
                newaccount.Save();

                //foreach (var account in EntityFactory.GetRepository<IAccount>().FindAll())
                //{
                //    //do something useful with the entity instance
                //    MessageBox.Show(account.ToString());
                //    //account
                //}
            }

        }
    }
}
