﻿namespace Multitenant.Core.Contracts
{
    public interface IMustHaveTenant
    {
        public string TenantId { get; set; }
    }
}
