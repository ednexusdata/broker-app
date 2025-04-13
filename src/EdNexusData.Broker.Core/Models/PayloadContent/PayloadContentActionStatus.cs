// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.ComponentModel;

namespace EdNexusData.Broker.Core;

public enum PayloadContentActionStatus
{
    [Description("Ready")]
    Ready,

    [Description("Waiting to Prepare")]
    WaitingToPrepare,

    [Description("Preparing")]
    Preparing,

    [Description("Prepared")]
    Prepared,

    [Description("Mapped")]
    Mapped,

    [Description("Waiting to Import")]
    WaitingToImport,

    [Description("Importing")]
    Importing,

    [Description("Imported")]
    Imported,

    [Description("Error")]
    Error
}