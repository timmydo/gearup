using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IKeyValueEntity
    {
		string Key { get; set; }
		string Value { get; set; }

		Task<bool> UpdateAsync();
	}
}
