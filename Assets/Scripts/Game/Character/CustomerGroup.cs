using System.Collections.Generic;

public class CustomerGroup
{
    public readonly List<CustomerCharacter> Members;
    public readonly int GroupSize;
    public DiningTable AssignedTable { get; set; }

    public CustomerGroup(int size)
    {
        GroupSize = size;
        Members = new List<CustomerCharacter>(size);
    }

    public void AddMember(CustomerCharacter customer)
    {
        Members.Add(customer);
    }

    public bool IsComplete => Members.Count == GroupSize;
}
