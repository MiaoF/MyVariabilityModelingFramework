    
//******************************************************************************
// NAME: PresenterFault.cs
//******************************************************************************
//
// Description: Simple DataContract to exchange prepared Information
//              from the Presenter to the Client Application.
//
//******************************************************************************
// Copyright (C) Siemens AG 2008. Confidential. All rights reserved.
//******************************************************************************
using System.Runtime.Serialization;

namespace Siemens.WarehouseManagement.ContractInfrastructure
{
  /// <summary>
  /// Simple DataContract to exchange prepared Information
  ///  from the Presenter to the Client Application.
  /// </summary>
  [DataContract]
  public class PresenterFault
  {
    /// <summary>
    /// Gets or sets the PresenterFault message
    /// </summary>
    /// <value>The message of the PresenterFault.</value>
    [DataMember]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the PresenterFault type. Differs between Exceptions and Messages
    /// </summary>
    /// <value>The type of the PresenterFault.</value>
    [DataMember]
    public FaultType Type { get; set; }

    /// <summary>
    /// Enumeration for all PresenterFault Types.
    /// </summary>
    public enum FaultType
    {
      /// <summary>
      /// If the PresenterFault has the FaultType 'Exception' there should appear an error icon in the dialog. This is default.
      /// </summary>
      Exception = 0,
      /// <summary>
      /// If the PresenterFault has the FaultType 'Message' there should appear an info icon in the dialog.
      /// </summary>
      Message = 1,
    }
  }
}
//******************************************************************************
// Copyright (C) Siemens AG 2008. Confidential. All rights reserved.
//*****************************************************************************



