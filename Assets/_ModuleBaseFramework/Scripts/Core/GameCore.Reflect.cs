using System;
using System.Reflection;

namespace ModuleBased
{
    public partial class GameCore
    {
        #region -- Assign required element methods --
        private void AssignLogger(IGameModule module)
        {
            module.Logger = _logger;
        }

        private void AssignLogger(IGameView view)
        {
            view.Logger = _logger;
        }

        private void AssignRequiredModules()
        {
            foreach (var modInst in Modules)
            {
                Type modType = modInst.GetType();
                AssignRequiredModule(modType, modInst);
            }
        }

        private void AssignRequiredModule(Type modType, IGameModule modInst)
        {
            MemberInfo[] members = modType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members)
            {
                if (member.IsDefined(typeof(RequireModuleAttribute)))
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(modInst, Modules.GetModule(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(modInst, Modules.GetModule(reqType));
                    }
                }
            }
        }

        private void AssigneRequiredDaos()
        {
            foreach (var modInst in Modules)
            {
                Type modType = modInst.GetType();
                AssignRequiredDao(modType, modInst);
            }
        }

        private void AssignRequiredDao(Type modType, IGameModule modInst)
        {
            MemberInfo[] members = modType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (member.IsDefined(typeof(RequireDaoAttribute)))
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(modInst, Daos.GetDao(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(modInst, Daos.GetDao(reqType));
                    }
                }
            }
        }
        #endregion
    }
}
