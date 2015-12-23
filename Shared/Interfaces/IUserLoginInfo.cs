using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IUserLoginInfo
    {
		DateTime Date { get; set; }
		string UserId { get; set; }
		string FriendlyName { get; set; }
		string Hmac { get; set; }
	}
}
