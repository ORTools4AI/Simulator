<!-- Modified for ORTools4AI.Simulator. -->
# Warehouse Simulation Model Specification

## Overview

This specification defines a **discrete-event simulation (DES) model** for warehouse operations. The model covers the full operational flow of a distribution or manufacturing-support warehouse: inbound receiving, putaway/storage, order picking, and outbound shipping. It supports multiple automation levels (manual, semi-automated, and fully automated AS/RS) and is designed to be implemented using the DESMOJ C# simulation framework.

The purpose of the simulation is to:

- Evaluate throughput capacity and resource utilisation under current and alternative configurations.
- Identify bottlenecks in receiving, picking, and shipping processes.
- Compare storage strategies (dedicated, random, class-based, ABC / High-Runner).
- Quantify the impact of changes in staffing, equipment, layout, or management policy before physical implementation.

---

## Warehouse Physical Structure

### Layout Zones

A warehouse is divided into the following functional zones:

| Zone | Description |
|---|---|
| **Receiving dock** | Truck/lorry arrival bays; goods are unloaded here |
| **Receiving/acceptance area** | Quantitative and qualitative inspection of incoming goods |
| **Bulk storage (Zone A)** | Large/heavy items stored directly on the floor on pallets |
| **Rack storage (Zone B)** | Small-to-medium items stored on shelving racks |
| **Picking area** | Workstations where items are picked from storage for assembly into orders |
| **Staging / packing area** | Temporary holding and packing before shipment |
| **Shipping dock** | Outbound truck/lorry bays |
| **Buffer / cross-docking area** | Optional: goods transferred directly from inbound to outbound without storage |

### Rack Structure

Rack storage is defined hierarchically:

```
Warehouse
  └── Rack row (aisle-facing group of racks)
        └── Rack (physical unit)
              └── Rack cell (vertical column of positions)
                    └── Rack level (height tier)
                          └── Rack position (individual storage slot)
```

**Parameters per rack row:**

| Parameter | Description |
|---|---|
| `num_racks_per_row` | Number of rack units in one row |
| `num_levels` | Number of vertical storage levels per rack |
| `num_positions_per_level` | Number of positions across one level |
| `position_width_m` | Width of a single position (metres) |
| `position_depth_m` | Depth of a single position (metres) |
| `position_height_m` | Height of a single storage level (metres) |
| `aisle_width_m` | Width of the handling aisle between rack rows |
| `side_access` | `One` (racks on one side of aisle) or `Both` (racks on both sides) |

### Channel-Based Supermarket (Alternative to Racks)

For line-supply supermarkets (e.g., Bosch-style kitting warehouses), storage is organised as depth channels rather than single positions:

| Parameter | Description |
|---|---|
| `num_corridors` | Number of travel lanes for milk-run vehicles |
| `racks_per_corridor` | Number of rack columns accessible from the corridor |
| `channels_per_column` | Number of depth channels in one rack column |
| `channel_capacity` | Maximum containers a single channel can hold (depth) |
| `channel_length_m` | Physical depth of the channel |
| `channel_width_m` | Width of the channel opening |
| `channel_height_m` | Height of the channel opening |

---

## Automation Levels

| Level | Description | Typical Equipment |
|---|---|---|
| **Manual** | All picking and transport by workers on foot or with hand trucks | Workers, hand pallet trucks |
| **Semi-automated** | Forklifts for bulk movement; manual picking for individual items | Electric forklifts, reach trucks, order pickers |
| **AS/RS** | Automated Storage and Retrieval System with cranes or stacker cranes | AS/RS crane, conveyor belts, I/O points |
| **AGV-assisted** | Automated guided vehicles transport loads between zones | AGVs (rail-guided or free-navigation) |
| **Fully automated sorting** | Automated sortation system for high-volume piece-level picking | Circular conveyor, induction zone, sorter chutes |

### AS/RS Parameters

| Parameter | Value / Description |
|---|---|
| `max_speed_m_per_min` | Maximum AGV/crane travel speed (e.g., 160 m/min) |
| `acceleration_m_per_s2` | Acceleration of AGV/crane (e.g., 0.8 m/s²) |
| `put_retrieve_time_s` | Time to deposit or retrieve one unit load at a rack position (e.g., 9 s) |
| `num_AS_RS_cranes` | Number of crane units per AS/RS aisle |
| `num_IO_points` | Number of input/output conveyor points |

---

## Entities (Objects That Flow Through the System)

| Entity | Description |
|---|---|
| **Truck / Lorry** | Arrives at docks carrying supplier deliveries (inbound) or picks up customer orders (outbound) |
| **Pallet** | Unit load; contains one or more SKUs; primary transport unit for bulk storage |
| **Container** | Smaller unit stored in channel-based supermarkets; holds a quantity of a single SKU |
| **SKU / Item** | Individual stock-keeping unit; tracked for picking and inventory |
| **Order** | Customer or production-line demand; consists of one or more SKU lines |
| **Picking wave** | Batch of orders released to picking simultaneously |
| **Picker** | Human worker who walks/rides to pick items |
| **Milk run vehicle** | Vehicle (cart or electric tow) that cycles a fixed route to collect containers |

---

## Resources (Shared Capacity-Limited Elements)

| Resource | Description |
|---|---|
| **Forklift** | Moves pallets between receiving dock, bulk storage, and shipping dock |
| **Reach truck** | Stores and retrieves pallets at height in rack storage |
| **Order picker truck** | Elevates the operator to pick items at height |
| **AGV** | Automated vehicle transporting loads; modelled with speed and acceleration |
| **Worker / picker** | Human resource shared across multiple process steps |
| **Dock door** | Physical constraint on simultaneous truck loading/unloading |
| **Sorter lane** | One path through the automated sorting system |
| **Scanner / WMS terminal** | Information processing time; models IT-induced delays |

---

## Process Flows

### P1 — Inbound (Receiving)

```
Truck arrival
  → Queue at dock door
    → Dock assignment
      → Unloading (forklift + pallet)
        → Transport to acceptance area
          → Quantitative & qualitative inspection
            → Putaway decision (direct or buffer)
              → Transport to storage location
                → End
```

**Key service time distributions (from field measurements):**

| Activity | Distribution | Parameters |
|---|---|---|
| Truck inter-arrival time | Fitted (Beta, Exponential, or Erlang) | Calibrated per day-of-week |
| Forklift pallet load time | Uniform | [3 s, 5 s] |
| Forklift travel loaded | Uniform | [10 s, 20 s] |
| Forklift pallet unload at drop | Uniform | [3 s, 5 s] |
| Forklift empty return | Uniform | [10 s, 15 s] |
| Full duty cycle per pallet | Derived | 25–43 s |
| Acceptance / inspection per pallet | Constant | 10 min |
| Validation tolerance | — | ≤ 10% error vs. real system |

**Typical daily volumes:** 3–5 trucks/day; 5–32 pallets per truck.

---

### P2 — Putaway / Storage

```
Pallet or container available at acceptance area
  → Storage location decision (storage policy)
    → Transport to assigned location
      → Placement (forklift or AS/RS)
        → WMS update
          → End
```

**Storage location decision is controlled by the active storage policy (see Section: Storage Strategies).**

---

### P3 — Order Picking

```
Order released to warehouse
  → Pick list generated (WMS)
    → Picker / milk run assigned
      → Travel to first pick location
        → [Repeat: pick item, scan, travel to next location]
          → Transport to packing / staging area
            → Verification
              → End (ready for shipping)
```

**Key service time distributions:**

| Activity | Distribution | Parameters |
|---|---|---|
| Manual pick time per item | Triangular | min=0.2 min, mode=0.5 min, max=1.0 min |
| Order picking per piece | Triangular | ~1:28–2:20 min (company-specific) |
| Order picking per package | Triangular | ~1:27–3:11 min (company-specific) |

**Picking strategies:**

| Strategy | Description |
|---|---|
| **Discrete picking** | One order at a time; picker completes one order before starting another |
| **Batch picking** | Picker collects items for multiple orders simultaneously; separated at packing |
| **Zone picking** | Warehouse divided into zones; each picker works only in their zone |
| **Wave picking** | Orders released in time-batched waves (e.g., 2-hour picking slots) |
| **Milk run** | Picker rides a vehicle along a fixed corridor circuit, collecting containers |

---

### P4 — Outbound (Shipping)

```
Order complete in staging area
  → Loading order issued
    → Dock assignment
      → Forklift loads pallets onto truck
        → Checking / verification
          → Truck departure
            → End
```

**Key service time distributions:**

| Activity | Distribution | Parameters |
|---|---|---|
| Customer lorry inter-arrival | Beta | Calibrated per system |
| Delivery order processing | Uniform | ~3.8 min |
| Order picking, sealing & loading (OPSL) | Triangular | min=28 min, mode=46–88 min, max=210 min |
| Loading per lorry | Triangular | min=28 min, mode=118 min, max=210 min |
| Final checking | Exponential or Normal | ~3.2 min |
| Pallet storage process | — | 1:46–3:30 per pallet |
| Shipping per pallet | — | ~4:03–4:11 per pallet |

---

### P5 — Automated Sorting (Optional)

Applicable to warehouses with an automated sorter (e.g., uniform kit assembly):

```
Items picked from storage → placed in transport boxes
  → Transport boxes to induction zone
    → Items removed from boxes → inducted onto sorter
      → Sorter routes items to chutes by kit/order
        → Complete kit exits chute
          → Packing and labelling
            → End
```

**Picking wave configuration:**

| Parameter | Value |
|---|---|
| Wave duration | 2 hours |
| Replications per experiment | 92 |
| Initial inventory fill level | 85% of storage locations |
| Target throughput | 600 uniform kits (40–60 SKUs each) per 8-hour shift |

---

## Storage Strategies

### Strategy A — Dedicated (Single-Product)

Each storage channel or rack position is permanently assigned to one SKU. Pickers can memorise locations. Requires the most space; guarantees FIFO per channel.

- **Advantage:** zero picking errors; always retrieve from front of channel.
- **Disadvantage:** lowest space utilisation (many positions partially empty).

### Strategy A2 — Dedicated with Zone Splitting

Variation of A where the warehouse is divided into zones, each served by a dedicated milk run or picker. Reduces average travel distance per shift.

### Strategy B — Date-Ordered Multi-Product

Multiple SKU types may share a channel if the incoming container's send-date is later than all existing containers. A **time gap** (safety interval between different SKU types) and a **max-types-per-channel** limit are enforced.

- **Risk:** depth > 1 if container send-dates and consumption dates are not synchronised.

### Strategy C — Consumption-Date Multi-Product

Containers are stored ordered by predicted consumption date; containers with a later consumption date may enter a channel regardless of SKU type, subject to:
- A configurable `time_gap` (hours) between types.
- A configurable `max_types_per_channel` limit.
- A prediction error `error_hours` applied to consumption date forecasts.

**Best observed global classification score: 91%** (strategy C, 0 h gap, 0 h error, 6 types/channel).  
**Reduction vs. strategy A:** ~30 s/trip (12%), ~55% fewer milk-run stops, ~69% fewer channels used.

### Strategy C2 — Consumption-Date Multi-Product (with non-current-week stock)

Identical to C but includes containers not consumed in the current work week. Slightly worse space efficiency. Best observed score: 71%.

### Class-Based Storage (ABC / XYZ)

SKUs are classified into classes based on turnover frequency:

| Class | Turnover | Storage zone |
|---|---|---|
| A (High-runner) | Highest demand frequency | Closest to I/O point / picking station |
| B (Core) | Medium demand | Middle of warehouse |
| C (Trail) | Lowest demand / slow-movers | Furthest from I/O point |

Items with the shortest delivery period (most frequently requested) are placed nearest the picking point; rarely-used items are stored furthest away. High-Runner Strategy minimises total picking transport time.

**Space rule:** within one rack position, only one SKU type is allowed. Products are oriented for maximum density (length-wise or width-wise as determined by experiment).

### Random Storage

Items are assigned to any available location without regard to SKU type or turnover. Simplest WMS logic; generally lowest space utilisation efficiency. Used as a baseline comparison.

---

## Performance Indicators (KPIs)

### Throughput

| KPI | Description |
|---|---|
| `pallets_received_per_shift` | Pallets accepted through the inbound process per working shift |
| `items_picked_per_wave` | Total units processed per picking wave |
| `orders_shipped_per_day` | Completed customer orders dispatched per day |
| `lorries_served_per_shift` | Number of trucks fully processed (in + out) per shift |

### Time-Based

| KPI | Description |
|---|---|
| `avg_lorry_residence_time_min` | Average total time a truck spends at the warehouse (wait + service) |
| `avg_wait_time_at_dock_min` | Average queue time before a dock door becomes available |
| `avg_picking_time_per_shift_s` | Average total time a picker/milk-run spends in a picking shift |
| `avg_order_cycle_time_min` | Time from order release to order ready for shipment |
| `avg_pallet_unload_time_s` | Average time to unload one pallet from a truck |
| `picking_wave_duration_min` | Wall-clock time to complete one full wave |

### Utilisation

| KPI | Description |
|---|---|
| `forklift_utilisation` | Fraction of shift time a forklift is actively transporting (0–1) |
| `picker_utilisation` | Fraction of shift time a picker is actively picking or travelling (0–1) |
| `dock_utilisation` | Fraction of shift time each dock door is occupied |
| `sorter_utilisation` | Fraction of wave time each sorter lane is active (0–1) |
| `AGV_utilisation` | Fraction of shift time each AGV is transporting (0–1) |
| `storage_fill_rate` | Percentage of storage positions occupied at a point in time |

### Space

| KPI | Description |
|---|---|
| `unused_channels` | Number of storage channels never accessed during simulation |
| `avg_pick_depth` | Average position-depth from which containers are retrieved (1 = front; > 1 = FIFO violation) |
| `shelf_space_m2` | Total floor area consumed by rack storage |
| `storage_capacity_pallet_places` | Total number of pallet positions available |

---

## Simulation Experiments and Scenarios

### Global Experiment Parameters

| Parameter | Description |
|---|---|
| `simulation_duration` | Total simulated time (e.g., 1 week, 8 h shift, 240 min) |
| `warm_up_period` | Period before statistics are collected (eliminates initial transient) |
| `num_replications` | Number of independent runs per scenario (≥ 10; 92 used in literature) |
| `seed` | Random number seed for reproducibility |
| `confidence_level` | Statistical confidence level for output intervals (default: 95%) |

### Predefined Scenarios

| Scenario | Description |
|---|---|
| **SC1 — Baseline** | Current system: existing layout, staffing, and storage policy. |
| **SC2 — Scheduled Truck Arrivals** | Regularise customer truck inter-arrival to 20 min (reduces OPSL queue). |
| **SC3 — Additional Forklift + Driver** | Add one forklift and driver to the order-picking/loading process. |
| **SC4 — Dedicated Storage (Strategy A)** | Single-product channel assignment; FIFO strict. |
| **SC5 — Class-Based Storage (Strategy C)** | Consumption-date multi-product; time_gap=0, error=0, max_types=6. |
| **SC6 — Zone-Divided Milk Runs** | Warehouse split into N zones; each zone served by one dedicated milk run. |
| **SC7 — AS/RS Integration** | Replace manual storage with AS/RS crane; AGV transports to picking area. |
| **SC8 — Automated Sorting** | Add sorter system; 4 picking stations; wave-based picking; target ≥ 9000 items/wave. |
| **SC9 — High-Runner Optimisation** | Apply genetic algorithm to assign SKUs to rack positions; minimise total picking time. |
| **SC10 — Narrow Aisles + More Racks** | Redesign rack layout with narrow aisles; increases pallet places by ~50%. |

---

## Input Data

### Warehouse Topology

```
topology:
  zones: [receiving, zone_A, zone_B, picking, staging, shipping, crossdock]
  rack_rows:
    - row_id: "R01"
      num_racks: 18
      num_levels: 3
      positions_per_level: 3
      position_size_m: [0.23, 0.42, 0.58]   # length, width, height
      aisle_width_m: 2.5
      side_access: Both
  channels:  # for supermarket-style layout
    - corridor_id: "C01"
      num_rack_columns: 10
      channels_per_column: 3
      channel_capacity: 6
      channel_size_m: [0.23, 0.42, 0.58]
```

### Products / SKUs

```
products:
  - sku_id: "SKU001"
    family: "Furnishing"         # product family / ABC class
    abc_class: "A"               # High-runner | Core | Trail
    avg_demand_per_day: 120      # units
    pallet_quantity: 40          # units per pallet
    weight_kg: 15.0
    size_m3: 0.05
    initial_inventory: 480       # units (= 85% of allocated positions)
```

### Arrivals

```
inbound_arrivals:
  inter_arrival_distribution: "Exponential"
  mean_inter_arrival_min: 60     # minutes between supplier trucks
  trucks_per_day_by_weekday: [5, 4, 4, 2, 3, 0, 0]   # Mon–Sun
  pallets_per_truck_distribution: "Uniform"
  pallets_per_truck_min: 5
  pallets_per_truck_max: 32

outbound_arrivals:
  inter_arrival_distribution: "Beta"
  inter_arrival_params: [alpha, beta, min, max]    # from input analyser fit
  lorries_per_day: 24
```

### Resources

```
resources:
  forklifts: 2
  reach_trucks: 1
  order_picker_trucks: 1
  pickers: 4
  dock_doors_inbound: 2
  dock_doors_outbound: 3
  agv_units: 1
  sorter_lanes: 4
```

### Service Time Distributions

```
service_times:
  forklift_load_s:        {dist: Uniform, min: 3,   max: 5}
  forklift_travel_s:      {dist: Uniform, min: 10,  max: 20}
  forklift_unload_s:      {dist: Uniform, min: 3,   max: 5}
  forklift_return_s:      {dist: Uniform, min: 10,  max: 15}
  acceptance_per_pallet_min: {dist: Constant, value: 10}
  manual_pick_per_item_min:  {dist: Triangular, min: 0.2, mode: 0.5, max: 1.0}
  opsl_per_lorry_min:        {dist: Triangular, min: 28,  mode: 46.2, max: 210}
  delivery_order_min:        {dist: Uniform,  min: 3.0, max: 4.5}
  final_check_min:           {dist: Normal,   mean: 3.2, std: 0.5}
  put_retrieve_AS_RS_s:      {dist: Constant, value: 9}
```

### Storage Strategy Configuration

```
storage_strategy:
  type: "C"               # A | A2 | B | C | C2 | Random | ClassBased
  time_gap_h: 0           # Strategy B/C: min hours between different SKU types in channel
  max_types_per_channel: 6  # Strategy B/C: max distinct SKU types per channel
  error_h: 0              # Strategy C: std of Gaussian forecast error on consumption date
  abc_zones: true         # ClassBased: enable A/B/C zone assignment
  fifo_strict: true       # Enforce FIFO within each channel
```

---

## Validation Criteria

A simulation model is considered valid when:

1. **Throughput error ≤ 10%:** `|output_simulation - output_real| / output_real ≤ 0.10` for each main process (pallets received, orders picked, lorries served).
2. **Resource utilisation error ≤ 10%:** Model utilisation rates match observed rates within 10 percentage points.
3. **Warm-up identification:** Run the model without warm-up and plot queue length over time; truncate data from the initial transient before computing statistics.
4. **Variance reduction:** Use common random numbers across alternative scenarios when comparing configurations.
5. **Minimum replications:** Run at least 10 independent replications; increase until the half-width of the 95% confidence interval is < 5% of the mean for key KPIs.

---

## Implementation Notes (DESMOJ)

The simulation model is implemented in **C# using the DESMOJ framework** (`src/ORTools4AI.Simulator/`). Map warehouse concepts to DESMOJ primitives as follows:

| Warehouse concept | DESMOJ element |
|---|---|
| Truck / lorry | `SimProcess` (lifecycle: arrive → queue → dock → depart) |
| Pallet / container / order | `Entity` (passive item flowing through queues) |
| Forklift / picker / AGV | `Res` (capacity-limited resource acquired with `provide()`) |
| Dock door | `Res` with capacity = number of doors |
| Sorter lane | `Res` with capacity = number of lanes |
| FIFO queue at dock | `WaitQueue` / `ServiceStation` |
| Picking wave timer | Scheduled `Event` fired every `wave_duration` |
| Milk-run corridor trip | `SimProcess.lifeCycle()` loop: travel → pick → travel → … |
| AS/RS crane | `SimProcess` with position state; travel time computed from kinematics |
| WMS location table | Dictionary `<string, SKU>` updated on each putaway / pick |
| Statistics | `Tally` (cycle times), `Accumulate` (queue lengths), `Count` (throughput) |

### SimProcess Pattern for a Forklift

```csharp
public override IEnumerable<RunStatus> lifeCycle()
{
    while (true)
    {
        // Wait for a pallet request
        yield return RunStatus.passivate;

        // Travel to pallet location
        hold(new TimeSpan(travelTime_s, TimeUnit.SECONDS));
        yield return RunStatus.passivate;

        // Load pallet
        hold(new TimeSpan(loadTime_s, TimeUnit.SECONDS));
        yield return RunStatus.passivate;

        // Travel to destination
        hold(new TimeSpan(travelTime_s, TimeUnit.SECONDS));
        yield return RunStatus.passivate;

        // Unload
        hold(new TimeSpan(unloadTime_s, TimeUnit.SECONDS));
        yield return RunStatus.passivate;

        // Return empty
        hold(new TimeSpan(returnTime_s, TimeUnit.SECONDS));
        yield return RunStatus.passivate;
    }
}
```

### Experiment Setup

```csharp
var model   = new WarehouseModel(null, "WarehouseSim", true, true);
var exp     = new Experiment("SC1_Baseline", TimeUnit.SECONDS, TimeUnit.MINUTES, null);
model.connectToExperiment(exp);
exp.stop(new TimeInstant(480, TimeUnit.MINUTES));  // 8-hour shift
exp.start();
exp.report();
exp.finish();
```

---

## References

1. Vieira et al. (2018). *Simulation model generation for warehouse management: case study to test different storage strategies.* Int. J. Simulation and Process Modelling, 13(4), 324–336.
2. Peixoto et al. *An automated warehouse design validation using discrete simulation.* (Brazilian Air Force AS/RS study; ARENA).
3. Živičnjak et al. (2022). *Case-study analysis of warehouse process optimization.* Transportation Research Procedia, 64, 215–223. (FlexSim; three Croatian warehouses.)
4. Kluska K. (2021). *Automatic simulation modelling of warehouses.* LogForum, 17(1), 59–69. (LogABS; drawer-rack SRS vs. row-rack SRR.)
5. *(Garbled encoding — AS/RS analytical model with kinematics and throughput formulas.)*
6. Liong & Loo (2009). *A simulation study of warehouse loading and unloading systems using ARENA.* JQMA, 5(2), 45–56.
7. Galé et al. (2002). *Simulation tool for managing a non-automated distribution warehouse.* Proc. 14th European Simulation Symposium.
8. Saderova et al. (2022). *Case study: the simulation modelling of selected activity in a warehouse operation.* Wireless Networks, 28, 431–440. (EXTENDSIM8; goods receipt.)
9. Fedorko et al. (2020). *The use of a simulation model for High-Runner Strategy implementation in warehouse logistics.* Sustainability, 12, 9818. (Tecnomatix Plant Simulation; genetic algorithm.)
