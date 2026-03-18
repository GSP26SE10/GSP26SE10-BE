using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Enum
{
    public enum OrderStatus
    {
        PENDING = 1,
        APPROVED = 2,
        REJECTED = 3,
        PREPARING = 4,
        IN_PROGRESS = 5,
        BILLING = 6,
        COMPLETED = 7,
        CANCELLED = 8
    }
}
