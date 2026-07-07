public class ServerStaffCharacter : StaffCharacter
{
    protected override bool CanHandle(WorkType type)
    {
        return type == WorkType.TakeOrder
            || type == WorkType.ServeFood
            || type == WorkType.ClearPlate;
    }

    protected override void EnqueueWork(WorkItem item)
    {
        switch (item)
        {
            case TakeOrderWorkItem takeOrder:
                EnqueueAction(new InteractAction(Mover, takeOrder.Table, this, Animator));
                break;

            case ServeFoodWorkItem serveFood:
                EnqueueAction(new InteractAction(Mover, serveFood.Counter, this, Animator));
                EnqueueAction(new InteractAction(Mover, serveFood.Table, this, Animator));
                EnqueueAction(new PutDownPlateAction(this, ServingCounters, KitchenCounters));
                break;

            case ClearPlateWorkItem clearPlate:
                EnqueueAction(new InteractAction(Mover, clearPlate.Table, this, Animator));
                EnqueueAction(new PutDownPlateAction(this, ServingCounters, KitchenCounters));
                break;
        }
    }
}
