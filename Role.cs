using Microsoft.AspNetCore.Identity;

namespace SprintOneStore.Models;

public sealed class Role : IdentityRole<Guid>
{
    public Role() { }
    public Role(string name) : base(name) { }
}
