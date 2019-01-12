﻿using MrCMS.Web.Apps.Admin.Infrastructure.Breadcrumbs;

namespace MrCMS.Web.Apps.Admin.Breadcrumbs
{
    public class UsersBreadcrumb : Breadcrumb
    {
        public override int Order => 99;
        public override string Controller => "";
        public override string Action => "";
        public override bool IsPlaceHolder => true;
        public override bool IsNav => true;
        public override string CssClass => "fa fa-users";
    }
}