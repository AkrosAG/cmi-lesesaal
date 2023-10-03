namespace CMI.Contract.Common
{
    public struct AccessRoles
    {
        public static string RoleOe1 => AccessRolesEnum.Ö1.ToString();
        public static string RoleOe2 => AccessRolesEnum.Ö2.ToString();
        public static string RoleOe3 => AccessRolesEnum.Ö3.ToString();
        public static string RoleEMA => AccessRolesEnum.EMA.ToString();
        public static string RoleAS => AccessRolesEnum.AS.ToString();
        public static string RoleAMA => AccessRolesEnum.AMA.ToString();
        public const string RoleMgntAllow = "ALLOW";
        public const string RoleMgntAppo = "APPO";
    }
}