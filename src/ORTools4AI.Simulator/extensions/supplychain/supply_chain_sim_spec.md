# Supply Chain Simulation Library — Spec

> Based on: Biswas & Narahari, "Object Oriented Modeling and Decision Support for Supply Chains" (EJOR 2004)

## 1. Design Principles

The library is a reusable toolkit of DESMOJ-based components for assembling discrete-event supply chain simulations. It follows three rules:

1. **Structural/policy separation** — Structural objects are pure data containers describing the physical network. Policy objects (attached to structural objects) define the behavioral logic. No structural object contains simulation or decision logic.
2. **Processes own the simulation** — DESMOJ `SimProcess` subclasses (processes) read from structural objects and invoke attached policies. Structural objects do not extend `SimProcess`.
3. **Composability via Strategy pattern** — Every behavioral dimension is expressed as an interface. Concrete implementations are plugged in at model-assembly time via `AttachPolicy`; processes invoke the interface without knowing the concrete type.

   The five behavioral dimensions and their interfaces:

   | Dimension | Interface | Who calls it |
   |---|---|---|
   | Inventory replenishment | `IInventoryPolicy` | `ReplenishmentProcess` |
   | Demand generation | `IDemandPolicy` | `DemandProcess` |
   | Manufacturing | `IManufacturingPolicy` | `ProductionProcess` |
   | Transport lane selection | `ITransportPolicy` | `TransportProcess` |
   | Upstream sourcing | `ISourcingPolicy` | `ReplenishmentProcess` |

   **How it works in practice:** `ReplenishmentProcess` calls `policy.ShouldReview(node, item, now)` and `policy.DecideOrderQuantity(...)`, where `policy` is whatever implementation was attached to the node for that item. The process code never changes — only the attached policy does.

   **Swapping example — standalone vs. digital twin:**
   ```csharp
   // Standalone: warehouse uses a min-max rule
   model.AttachPolicy(warehouse, item, new MinMaxPolicy(s: 50, S: 200));

   // Digital twin: swap to the optimizer's plan with no other changes
   model.AttachPolicy(warehouse, item, new OptimizerPolicy(solution, periodDuration));
   ```
   `ReplenishmentProcess` is identical in both cases. The same principle applies to all other dimensions independently — demand policy, transport policy, and sourcing policy can each be swapped without affecting the others.

---

## 2. Library Layers

```
┌─────────────────────────────────────────────────────┐
│  SupplyChainModel  (DESMOJ Model subclass)           │
│  Builder API: AddNode / AddLane / AttachPolicy       │
├──────────────────┬──────────────────────────────────┤
│  Process Layer   │  Structural Layer                 │
│  (SimProcess)    │  (plain C# objects)               │
│                  │                                   │
│  DemandProcess        │  SupplyChainNode              │
│  FulfillProcess       │    Supplier                   │
│  ReplenishmentProcess │    Plant                      │
│  TransportProcess     │    Warehouse                  │
│  ProductionProcess    │    Retailer                   │
│                  │    Customer                       │
│                  │                                   │
│                  │  Order  (DESMOJ Entity)           │
│                  │  Item                             │
│                  │  TransportLane                    │
│                  │  InventoryLedger                  │
├──────────────────┴──────────────────────────────────┤
│  Policy Layer  (`policies/`)                         │
│                                                     │
│  IInventoryPolicy    IManufacturingPolicy           │
│  IDemandPolicy       ITransportPolicy               │
│  ISourcingPolicy                                    │
└─────────────────────────────────────────────────────┘
```

Processes read the structural layer and call policies. Policies read and mutate the structural layer (specifically `InventoryLedger`).

---

## 3. Structural Objects

### 3.1 `SupplyChainNode`  (`masterdata/SupplyChainNode.cs`)

Base class for all physical network nodes.

```
SupplyChainNode
  string NodeId
  string Name
  NodeType Type                                          // Supplier | Plant | Warehouse | Retailer | Customer
  Dictionary<Item, IInventoryPolicy> InventoryPolicies  // one entry per managed item; empty for Supplier/Customer
  Dictionary<Item, IManufacturingPolicy> ManufacturingPolicies  // one entry per finished good; non-empty for Plant only
  Dictionary<Item, IDemandPolicy> DemandPolicies        // one entry per demanded item; non-empty for Customer/Retailer
  ITransportPolicy? TransportPolicy                     // node-wide default; overridden per-lane by TransportLane.PolicyOverride
  ISourcingPolicy? SourcingPolicy                       // node-wide; handles all items via Item parameter
  InventoryLedger Inventory                             // on-hand + on-receipt per Item
  Queue<Order> PendingOrders                            // customer orders waiting to be filled; drained by FulfillProcess
```

`InventoryPolicies`, `ManufacturingPolicies`, and `DemandPolicies` are populated by `model.AttachPolicy(node, item, policy)`. Items are considered "managed" by a node if they appear as a key in one of these dictionaries or in `InventoryLedger` via `SetInitialInventory`.

Derived classes add no logic — only constructor defaults and documentation:

| Class | NodeType | Notes |
|---|---|---|
| `Supplier` | `Supplier` | External source; has capacity per item but no inventory or sourcing policy |
| `Plant` | `Plant` | Has `InventoryPolicies` (per raw material), `ManufacturingPolicies` (per finished good), `SourcingPolicy` |
| `Warehouse` | `Warehouse` | Has `InventoryPolicies` (per item) + `SourcingPolicy` |
| `Retailer` | `Retailer` | Has `InventoryPolicies` + `DemandPolicies` + `SourcingPolicy` |
| `Customer` | `Customer` | Has `DemandPolicies` only; no inventory or sourcing |

### 3.2 `Item`  (`masterdata/Item.cs`)

```
Item
  string ItemId
  string Name
  double UnitCost
  double SalesPrice
  bool IsRawMaterial
```

### 3.3 `Order`  (`transactionaldata/Order.cs`)  — DESMOJ `Entity`

```
Order : Entity
  string OrderId
  Item Item
  double Quantity
  SupplyChainNode Origin
  SupplyChainNode Destination
  OrderType Type          // CustomerOrder | ReplenishmentOrder | ProductionOrder
  OrderStatus Status      // Created | InTransit | PartiallyFilled | Fulfilled | Lost
  TimeInstant PlacedAt
  TimeInstant? DueAt      // optional deadline; null = no deadline
```

### 3.4 `TransportLane`  (`masterdata/TransportLane.cs`)

Directed arc between two nodes for a specific item.

```
TransportLane
  SupplyChainNode From
  SupplyChainNode To
  Item Item
  NumericalDist<double> LeadTimeDist   // DESMOJ distribution for transit time
  double Capacity                       // max units per shipment; 0 = unlimited
  double CostPerUnit
  ITransportPolicy? PolicyOverride      // overrides node-level ITransportPolicy if set
```

### 3.5 `InventoryLedger`  (`transactionaldata/InventoryLedger.cs`)

Per-node ledger, keyed by `Item`. Replaces the existing `InventoryData` / `TwoDimArray`.

```
InventoryLedger
  double OnHand(Item item)
  double OnReceipt(Item item)
  void AdjustOnHand(Item item, double delta)
  void AdjustOnReceipt(Item item, double delta)
  IEnumerable<Item> TrackedItems()
```

---

## 4. Policy Interfaces

All policies use the **Strategy pattern** and are attached to structural objects via `SupplyChainNode` properties.

### 4.1 `IInventoryPolicy`  (`policies/inventory/IInventoryPolicy.cs`)

Called by `ReplenishmentProcess` and `ProductionProcess` to decide when and how much to order.

```
IInventoryPolicy
  bool ShouldReview(SupplyChainNode node, Item item, TimeInstant now)
  double DecideOrderQuantity(SupplyChainNode node, Item item, TimeInstant now)
```

Built-in implementations:

| Class | Logic |
|---|---|
| `MinMaxPolicy` | (s, S): order `S − onHand` when `onHand < s` |
| `FixedOrderQuantityPolicy` | (r, Q): order fixed `Q` when `onHand ≤ r` |
| `BaseStockPolicy` | Order exactly what was consumed since last review to restore base-stock level `B` |

### 4.2 `IManufacturingPolicy`  (`policies/production/IManufacturingPolicy.cs`)

Called by `ProductionProcess` to decide production quantities and mode.

```
IManufacturingPolicy
  ManufacturingMode Mode          // MakeToStock | MakeToOrder | AssembleToOrder
  double DecideProductionQuantity(Plant plant, Item finishedGood, TimeInstant now)
  IReadOnlyDictionary<Item, double> GetBomRequirements(Item finishedGood, double qty)
  NumericalDist<double> ProductionLeadTimeDist(Plant plant, Item finishedGood)
```

Built-in implementations:

| Class | Mode | Logic |
|---|---|---|
| `MakeToStockPolicy` | MakeToStock | Produce when finished-goods inventory falls below reorder point |
| `MakeToOrderPolicy` | MakeToOrder | Produce only when a customer order is received |
| `AssembleToOrderPolicy` | AssembleToOrder | Maintain component stock; assemble on order receipt |

### 4.3 `IDemandPolicy`  (`demand/IDemandPolicy.cs`)

Called by `DemandProcess` to generate customer demand arrivals.

```
IDemandPolicy
  NumericalDist<double> InterArrivalTimeDist   // time between successive demand arrivals for this item
  NumericalDist<double> OrderQuantityDist       // quantity per arrival
```

Each `IDemandPolicy` instance is scoped to a single item via the `DemandPolicies` dictionary key on `SupplyChainNode`. The item is not stored inside the policy itself.

Built-in implementations:

| Class | Arrival pattern | Quantity pattern |
|---|---|---|
| `PoissonDemandPolicy` | Exponential inter-arrival (`ContDistExponential`) | Fixed or Poisson quantity |
| `PeriodicDemandPolicy` | Fixed interval (`ContDistConstant`) | Configurable distribution |
| `TraceDrivenDemandPolicy` | Replay from a list of `(time, item, quantity)` records | Deterministic from trace |

### 4.4 `ITransportPolicy`  (`policies/transport/ITransportPolicy.cs`)

Called by `TransportProcess` to select a lane and schedule shipment.

```
ITransportPolicy
  TransportLane SelectLane(SupplyChainNode origin, SupplyChainNode destination,
                            Item item, IReadOnlyList<TransportLane> candidates)
  double DecideShipmentQuantity(TransportLane lane, Order order)
```

Built-in implementations:

| Class | Logic |
|---|---|
| `DirectShipPolicy` | Always use the single available lane; ship full order quantity |
| `CheapestLanePolicy` | Select the lane with the lowest `CostPerUnit` |
| `FastestLanePolicy` | Select the lane with the lowest expected lead time |

### 4.5 `ISourcingPolicy`  (`policies/sourcing/ISourcingPolicy.cs`)

Called by `ReplenishmentProcess` to decide which upstream node(s) supply a given node for a given item. Answers "where do I order from?" before `IInventoryPolicy` answers "how much?"

```
ISourcingPolicy
  IReadOnlyList<SupplyChainNode> GetCandidateSources(SupplyChainNode consumer, Item item,
                                                      IReadOnlyList<SupplyChainNode> available)
  IReadOnlyDictionary<SupplyChainNode, double> AllocateOrder(SupplyChainNode consumer, Item item,
                                                              double totalQuantity,
                                                              IReadOnlyList<SupplyChainNode> sources,
                                                              TimeInstant now)
```

- `GetCandidateSources` filters and ranks available upstream nodes.
- `AllocateOrder` splits the total replenishment quantity across one or more sources (returns a source → quantity map).

Built-in implementations:

| Class | Logic |
|---|---|
| `SingleSourcePolicy` | Always order from one fixed upstream node; all quantity goes there |
| `PrioritySourcePolicy` | Try sources in priority order; move to next if first has insufficient inventory |
| `SplitSourcePolicy` | Distribute quantity across sources in fixed proportion (e.g. 70 % / 30 %) |
| `NearestSourcePolicy` | Source from the upstream node with the highest on-hand inventory |

---

## 5. SimProcesses

Processes are DESMOJ `SimProcess` subclasses. They hold references to nodes and shared state but contain no supply-chain data themselves — all data lives in structural objects and policies.

Each process is instantiated per-node (or per-lane) by `SupplyChainModel.doInitialSchedules()` and activated at time 0.

### 5.1 `DemandProcess`  (`process/DemandProcess.cs`)

One instance per `(Customer or Retailer, Item)` pair where a `DemandPolicy` is attached. Each instance drives an independent demand stream for its item; concurrent instances on the same node produce independent Poisson (or other) arrival processes.

```
// DemandProcess holds: node, item, policy = node.DemandPolicies[item]
lifeCycle loop:
  1. t = policy.InterArrivalTimeDist.sample()
  2. hold(t)
  yield return passivate
  3. qty = policy.OrderQuantityDist.sample()
  4. Create Order(type=CustomerOrder, item, qty, origin=node, destination=node)
  5. Post Order to FulfillProcess of this node
  6. goto 1
```

### 5.2 `FulfillProcess`  (`process/FulfillProcess.cs`)

One instance per node that holds inventory and receives customer orders (`Warehouse`, `Retailer`).

```
lifeCycle loop:
  1. Passivate (wait for an Order posted by DemandProcess or ReplenProcess)
  yield return passivate

  2. Dequeue next Order from pendingOrders
  3. If node.Inventory.OnHand(order.Item) >= order.Quantity:
       node.Inventory.AdjustOnHand(order.Item, -order.Quantity)
       order.Status = Fulfilled
       Record fill event in statistics
  4. Else if backlog is enabled:
       Enqueue Order in backlogQueue
       order.Status = PartiallyFilled
       Record backlog event in statistics
  5. Else:
       order.Status = Lost
       Record lost-sale event in statistics
  6. goto 1
```

Reactivated by `TransportProcess` when a replenishment delivery arrives.

### 5.3 `ReplenishmentProcess`  (`process/ReplenishmentProcess.cs`)

One instance per node that has **both** `InventoryPolicy` and `SourcingPolicy` set (Warehouse, Retailer, Plant for raw materials). Nodes with neither (Customer, Supplier) do not get a `ReplenishmentProcess`.

```
lifeCycle loop:
  1. hold(reviewInterval)
  yield return passivate

  2. For each (item, policy) in node.InventoryPolicies:
       If policy.ShouldReview(node, item, now):
         qty = policy.DecideOrderQuantity(node, item, now)
         If qty > 0:
           sources    = node.SourcingPolicy.GetCandidateSources(node, item, availableUpstream)
           allocation = node.SourcingPolicy.AllocateOrder(node, item, qty, sources, now)
           For each (source, partialQty) in allocation:
             Create Order(type=ReplenishmentOrder, item, partialQty,
                          origin=source, destination=node)
             node.Inventory.AdjustOnReceipt(item, +partialQty)
             Post Order to TransportProcess for the source→node lane
  3. goto 1
```

### 5.4 `TransportProcess`  (`process/TransportProcess.cs`)

One instance per `TransportLane`. Accepts orders and schedules their arrival.

```
lifeCycle loop:
  1. Passivate (wait for an Order posted by ReplenishmentProcess or ProductionProcess)
  yield return passivate

  2. Dequeue next Order
  3. policy = lane.PolicyOverride ?? order.Origin.TransportPolicy ?? new DirectShipPolicy()
  4. qty = policy.DecideShipmentQuantity(lane, order)
  5. Sample lead time: lt = lane.LeadTimeDist.sample()
  6. Schedule ArrivalEvent at (now + lt) carrying the Order
  7. order.Status = InTransit
  8. Record shipment in statistics
  9. goto 1
```

`ArrivalEvent` (an `ExternalEvent`) fires at the scheduled time and calls `ReceiveDelivery(order)` on the destination node's `FulfillProcess` / `ProductionProcess`.

### 5.5 `ProductionProcess`  (`process/ProductionProcess.cs`)

One instance per `Plant`. Applies `ManufacturingPolicy` and `InventoryPolicy` for raw materials.

```
lifeCycle loop:
  1. hold(productionReviewInterval)
  yield return passivate

  2. For each (item, policy) in plant.ManufacturingPolicies:
       qty = policy.DecideProductionQuantity(plant, item, now)
       If qty > 0:
         bom = policy.GetBomRequirements(item, qty)
         If all bom raw materials available in plant.Inventory:
           Deduct raw material quantities from plant.Inventory
           lt = policy.ProductionLeadTimeDist(plant, item).sample()
           hold(lt)
           yield return passivate
           plant.Inventory.AdjustOnHand(finishedGood, +qty)
           Reactivate FulfillProcess of this plant
  3. goto 1
```

---

## 6. `SupplyChainModel`  (`SupplyChainModel.cs`)

Extends DESMOJ `Model`. Provides a builder API to assemble a simulation from components.

```csharp
var model = new SupplyChainModel(null, "MyChain", true, true);

// Add structural objects
var plant    = model.AddNode(new Plant("P1", "Main Plant"));
var warehouse = model.AddNode(new Warehouse("W1", "East Warehouse"));
var customer  = model.AddNode(new Customer("C1", "Retail Customer"));
var item      = model.AddItem(new Item("SKU-A", unitCost: 10, salesPrice: 25));

// Add transport lanes
model.AddLane(new TransportLane(plant, warehouse, item,
    leadTimeDist: new ContDistExponential(model, "LT_PW", 2.0, true, false),
    capacity: 0, costPerUnit: 1.5));
model.AddLane(new TransportLane(warehouse, customer, item,
    leadTimeDist: new ContDistUniform(model, "LT_WC", 0.5, 1.5, true, false),
    capacity: 0, costPerUnit: 0.5));

// Attach policies
// Overloads (item-scoped):
//   void AttachPolicy(SupplyChainNode node, Item item, IInventoryPolicy policy)
//   void AttachPolicy(SupplyChainNode node, Item item, IManufacturingPolicy policy)
//   void AttachPolicy(SupplyChainNode node, Item item, IDemandPolicy policy)
// Overloads (node-wide, no item param):
//   void AttachPolicy(SupplyChainNode node, ITransportPolicy policy)
//   void AttachPolicy(SupplyChainNode node, ISourcingPolicy policy)
model.AttachPolicy(warehouse, item, new MinMaxPolicy(s: 50, S: 200));
model.AttachPolicy(plant,     item, new MakeToStockPolicy(reorderPoint: 100, bom: new() { ["RM-A"] = 2.0 }));
model.AttachPolicy(customer,  item, new PoissonDemandPolicy(arrivalRate: 0.5, meanQty: 10));

// Initial inventory
model.SetInitialInventory(warehouse, item, onHand: 150, onReceipt: 0);
model.SetInitialInventory(plant, item, onHand: 300, onReceipt: 0);

// Connect to DESMOJ experiment
var exp = new Experiment("SC-Exp");
model.connectToExperiment(exp);
exp.stop(new TimeInstant(365, TimeUnit.DAYS));
exp.start();
exp.report();
exp.finish();
```

`SupplyChainModel.init()` instantiates distributions and statistics.
`SupplyChainModel.doInitialSchedules()` creates and activates one worker per node/lane.

---

## 7. Statistics & KPIs

The model instantiates DESMOJ statistics objects in `init()`. All are reported by `exp.report()`.

| Statistic | DESMOJ class | Granularity | Measures |
|---|---|---|---|
| Inventory level | `Accumulate` | Per (node, item) | Time-weighted average `OnHand` |
| Backlog level | `Accumulate` | Per (node, item) | Time-weighted average queued-unfilled quantity |
| Fill rate | `Count` (filled) + `Count` (total) | Per (node, item) | `filled / total` at end |
| Order lead time | `Tally` | Per (node, item) | Time from `Order.PlacedAt` to `Status = Fulfilled` |
| Lost sales | `Count` | Per (node, item) | Orders dropped when no backlog capacity |
| Throughput | `Count` | Per lane | Shipments completed per simulated period |

---

## 8. Order Communication Between Processes

Processes communicate by posting `Order` entities into shared queues. No process holds a direct reference to another process.

```
DemandProcess  ──posts Order──▶  pendingOrders queue  ◀── FulfillProcess dequeues
ReplenProcess  ──posts Order──▶  shipmentQueue queue  ◀── TransportProcess dequeues
TransportProcess ── fires ArrivalEvent ──▶  FulfillProcess.ReceiveDelivery()
ProductionProcess ── fires ArrivalEvent ──▶  FulfillProcess.ReceiveDelivery()
```

Each node owns its `pendingOrders` queue. Each lane owns its `shipmentQueue`. Processes reactivate each other only via these queues and arrival events — no direct `reActivate()` calls between processes of different types.

---

## 9. File Layout

```
extensions/supplychain/
├── spec.md                          ← this file
├── SupplyChainModel.cs
├── masterdata/                      ← locations and items (structural data)
│   ├── Item.cs
│   ├── SupplyChainNode.cs           (base + NodeType enum)
│   ├── Supplier.cs
│   ├── Plant.cs
│   ├── Warehouse.cs
│   ├── Retailer.cs
│   ├── Customer.cs
│   └── TransportLane.cs
├── transactionaldata/               ← runtime order and inventory state
│   ├── Order.cs
│   └── InventoryLedger.cs
├── policies/                        ← all policy interfaces and implementations
│   ├── inventory/
│   │   ├── IInventoryPolicy.cs
│   │   ├── MinMaxPolicy.cs
│   │   ├── FixedOrderQuantityPolicy.cs
│   │   └── BaseStockPolicy.cs
│   ├── production/
│   │   ├── IManufacturingPolicy.cs
│   │   ├── MakeToStockPolicy.cs
│   │   ├── MakeToOrderPolicy.cs
│   │   └── AssembleToOrderPolicy.cs
│   ├── transport/
│   │   ├── ITransportPolicy.cs
│   │   ├── DirectShipPolicy.cs
│   │   ├── CheapestLanePolicy.cs
│   │   └── FastestLanePolicy.cs
│   └── sourcing/
│       ├── ISourcingPolicy.cs
│       ├── SingleSourcePolicy.cs
│       ├── PrioritySourcePolicy.cs
│       ├── SplitSourcePolicy.cs
│       └── NearestSourcePolicy.cs
├── demand/                          ← demand policy (top-level; driven by DemandProcess, not review cycle)
│   ├── IDemandPolicy.cs
│   ├── PoissonDemandPolicy.cs
│   ├── PeriodicDemandPolicy.cs
│   └── TraceDrivenDemandPolicy.cs
└── process/
    ├── DemandProcess.cs
    ├── FulfillProcess.cs
    ├── ReplenishmentProcess.cs
    ├── TransportProcess.cs
    └── ProductionProcess.cs
```

---

## 10. What to Do with the Existing Skeleton

The existing files in `extensions/supplychain/` are replaced by this design. Delete them once the new classes are in place.

### Name mapping (old → new)

| Old class / concept | New class / concept |
|---|---|
| `Site` | `SupplyChainNode` (base) |
| `Store` | `Warehouse` |
| `Facility` | `Plant` |
| `ExternalCustomerProcess` | `DemandProcess` |
| `FullFillProcess` | `FulfillProcess` |
| `SourceProcess` | `ReplenishmentProcess` |
| `DeliveryProcess` | `TransportProcess` |
| `ReceiveProcess` | `ArrivalEvent` (fired inside `TransportProcess`) |
| `MakeProcess` | `ProductionProcess` |
| `InventoryData` / `TwoDimArray` | `InventoryLedger` |
| `IDemand<N,T>` | `IDemandPolicy` |

### Bugs in the existing skeleton (do not carry forward)

- **`TwoDimArray` set-indexer** (`InventoryData.cs`): falls through to unconditional `.Add()` even when the key exists, corrupting the dictionary on second write. `InventoryLedger` replaces this entirely.
- **`MakeProcess` not a `SimProcess`**: declared as a plain class; can never be scheduled. Replaced by `ProductionProcess : SimProcess`.
- **`FullFillProcess.lifeCycle()` missing `yield`**: `while(true) { }` with no `yield return passivate` hangs the scheduler. Fixed in the new `FulfillProcess`.
- **`ExternalCustomerProcess.lifeCycle()` exits immediately**: `yield break` inside `while(true)` exits the coroutine on the first iteration — no demand is ever generated. Fixed in the new `DemandProcess`.
