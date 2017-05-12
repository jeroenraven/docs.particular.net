using System;
using System.Threading.Tasks;

class Database
{
    public bool IsFailing { get; set; }

    public Task Store()
    {
        if (IsFailing)
        {
            throw new Exception("Database is down");
        }
        return Task.FromResult(0);
    }
}