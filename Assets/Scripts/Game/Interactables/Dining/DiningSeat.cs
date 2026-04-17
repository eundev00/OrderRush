using UnityEngine;
using VContainer;

public class DiningSeat : MonoBehaviour
{
    [SerializeField] Transform _sitPoint;

    private CustomerCharacter _currentCustomer;
    private int _seatIndex;

    public Transform SitPoint => _sitPoint;
    public bool HasCustomer => _currentCustomer != null;
    public CustomerCharacter CurrentCustomer => _currentCustomer;

    public void Init(DiningTable table, int index)
    {
        _seatIndex = index;
    }

    public void SeatCustomer(CustomerCharacter customer)
    {
        _currentCustomer = customer;
    }

    public void Clear()
    {
        _currentCustomer = null;
    }

    public int GetSeatIndex()
    {
        return _seatIndex;
    }

}
