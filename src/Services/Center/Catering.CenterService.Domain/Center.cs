using Catering.BuildingBlocks.Domain;

namespace Catering.CenterService.Domain;

public sealed class Center : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;

    private Center()
    {
    }

    public static Center Create(string name, string address) =>
        new() { Name = name, Address = address };

    public void Update(string name, string address)
    {
        Name = name;
        Address = address;
        Touch();
    }
}
