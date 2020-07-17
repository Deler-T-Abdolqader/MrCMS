using System.Collections.Generic;
using MrCMS.Entities.People;
using MrCMS.Models;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Services
{
    public interface IRoleAdminService
    {
        AddRoleResult AddRole(AddRoleModel model);
        UpdateRoleModel GetEditModel(int id);
        void SaveRole(UpdateRoleModel model);
        void DeleteRole(int id);
        IEnumerable<UserRole> GetAllRoles();
        IEnumerable<AutoCompleteResult> Search(string term);
        IEnumerable<string> GetRolesForPermissions();
    }
}