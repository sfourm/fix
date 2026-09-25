namespace Fix.Domain.AggregateRoots.Counterparties;

public enum CounterpartyType
{
    Trading = 1,

    /// <summary>Banco / trading (ex.: BTG Commodities).</summary>
    BankTrading = 2,

    /// <summary>Contraparte de derivativos de balcão.</summary>
    OtcCounterparty = 3,

    /// <summary>Corretora (FCM) de bolsa.</summary>
    Broker = 4,

    /// <summary>Transportadora (frete).</summary>
    Carrier = 5,

    /// <summary>Produtor rural (CPF).</summary>
    Producer = 6,
}

