<p align="center">
    <img src="https://raw.githubusercontent.com/bmresearch/Solnet.Serum/master/assets/icon.png" margin="auto" height="175"/>
</p>

<p align="center">
    <a href="https://github.com/bmresearch/Solnet.Serum/actions/workflows/dotnet.yml">
        <img src="https://github.com/bmresearch/Solnet.Serum/actions/workflows/dotnet.yml/badge.svg"
            alt="Build .NET6" ></a>
    <a href="https://github.com/bmresearch/Solnet.Serum/actions/workflows/publish.yml">
        <img src="https://github.com/bmresearch/Solnet.Serum/actions/workflows/publish.yml/badge.svg"
            alt="Release .NET6" ></a>
<br/>
    <a href="https://github.com/bmresearch/Solnet.Serum/actions/workflows/dotnet.yml">
        <img src="https://github.com/bmresearch/Solnet.Serum/actions/workflows/dotnet.yml/badge.svg?branch=net5"
            alt="Build .NET5" ></a>
    <a href="https://github.com/bmresearch/Solnet.Serum/actions/workflows/publish.yml">
        <img src="https://github.com/bmresearch/Solnet.Serum/actions/workflows/publish.yml/badge.svg?branch=net5"
            alt="Release .NET5" ></a>
    <a href="https://coveralls.io/github/bmresearch/Solnet.Serum?branch=master">
        <img src="https://coveralls.io/repos/github/bmresearch/Solnet.Serum/badge.svg?branch=master" 
            alt="Coverage Status" ></a>
<br/>
    <a href="">
        <img src="https://img.shields.io/github/license/bmresearch/solnet.serum?style=flat-square"
            alt="Code License"></a>
    <a href="https://twitter.com/intent/follow?screen_name=blockmountainio">
        <img src="https://img.shields.io/twitter/follow/blockmountainio?style=flat-square&logo=twitter"
            alt="Follow on Twitter"></a>
    <a href="https://discord.gg/YHMbpuS3Tx">
       <img alt="Discord" src="https://img.shields.io/discord/849407317761064961?style=flat-square"
            alt="Join the discussion!"></a>
</p>

# What is Solnet.Serum?

[Solnet](https://github.com/bmresearch/Solnet) is Solana's .NET integration library, a number of packages that implement features to interact with
Solana from .Net applications.

Solnet.Serum is a package within the same `Solnet.` namespace that implements a Client for [Serum](https://projectserum.com), this project is in a
separate repository so it is contained, as the goal for [Solnet](https://github.com/bmresearch/Solnet) was to be a core SDK.

## Features
- Decoding of Serum data structures:
  - `Market`
  - `OpenOrdersAccount`
  - `Slab`s, which are used for order book data stored under `OrderBookSide` and `OrderBook` which holds both sides
  - `EventQueue` (`Event` data, used to process and filter for `TradeEvent`s)
- `SerumProgram` instructions implemented:
  - `NewOrderV3`
  - `CancelOrderV2`
  - `CancelOrderByClientIdV2`
  - `SettleFunds`
  - `ConsumeEvents`
  - `InitOpenOrders`
  - `CloseOpenOrders`
  - `Prune`
- `SerumClient` class which allows to:
  - Get these structures and decode them only by having their address
  - Subscribing to these accounts in real time, getting notifications with their decoded structures
- `MarketManager` class which has:
  - Various overloads of `NewOrder`, `NewOrders`, `CancelOrder` and `CancelAllOrders`, these:
    - craft a transaction or several transactions, in the case where they interact with several orders
    - request a signature using the defined delegate method
    - submit the transaction to the cluster
    - and subscribe to the confirmation of the signature, notifying the user when it happens
    - if the transaction is subject to a custom error defined by the Serum Program [here](https://github.com/project-serum/serum-dex/blob/master/dex/src/error.rs), it is parsed into the appropriate `SerumProgramError` enum value
- Factory patterns for both `ISerumClient` and `IMarketManager`


## Requirements
- net 6.0

## Dependencies
- Solnet.Rpc v6.0.7
- Solnet.Wallet v6.0.7
- Solnet.Programs v6.0.7

## Examples

The [Solnet.Serum.Examples](https://github.com/bmresearch/Solnet.Serum/tree/master/Solnet.Serum.Examples) project features some examples on how to use both the [IMarketManager](https://github.com/bmresearch/Solnet.Serum/tree/master/Solnet.Serum/IMarketManager.cs) and the [ISerumClient](https://github.com/bmresearch/Solnet.Serum/tree/master/Solnet.Serum/ISerumClient.cs), these examples include:
- Streaming market data directly into user-friendly values using the `IMarketManager` interface
- Submitting new orders and cancelling existing ones

## Contribution

We encourage everyone to contribute, submit issues, PRs, discuss. Every kind of help is welcome.

## Contributors

* **Hugo** - *Maintainer* - [murlokito](https://github.com/murlokito)
* **Tiago** - *Maintainer* - [tiago](https://github.com/tiago18c)

See also the list of [contributors](https://github.com/bmresearch/Solnet.Serum/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/bmresearch/Solnet.Serum/blob/master/LICENSE) file for details
