﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitfiWallet
{
  public class XSplash_Zero
  {

    const string NOX_SPLASH = "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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD4DwAAAAAAAEFQSyBTaWcgQmxvY2sgNDJQSwECAAAAAAAACAAAAAAA7n0sVT8EAAAcDQAAEwAAAAAAAAAAAAAAAAAAAAAAQW5kcm9pZE1hbmlmZXN0LnhtbFBLAQIYABQAAAAIAAAAAAAJWVT1UwAAAFcAAAAUAAAAAAAAAAAAAAAAAHAEAABNRVRBLUlORi9NQU5JRkVTVC5NRlBLAQIYABQAAAAIAAAAAABONLkN0wUAAAgLAAALAAAAAAAAAAAAAAAAAPUEAABjbGFzc2VzLmRleFBLAQIAAAAAAAAIAAAAAACN628i0AAAAGwBAAAcAAAAAAAAAAAAAAAAAPEKAAByZXMvbGF5b3V0L2FjdGl2aXR5X21haW4ueG1sUEsBAgAAAAAAAAgAAAAAAI3rbyLQAAAAbAEAABsAAAAAAAAAAAAAAAAA+wsAAHJlcy9sYXlvdXQvY29udGVudF9tYWluLnhtbFBLAQIAAAAAAAAIAAAAAAAIRcDJigEAAGwEAAAYAAAAAAAAAAAAAAAAAAQNAAByZXMveG1sL2RldmljZV9hZG1pbi54bWxQSwECAAAAAAAAAAAAAAAA8K++vpgGAACYBgAADgAAAAAAAAAAAAAAAADEDgAAcmVzb3VyY2VzLmFyc2NQSwUGAAAAAAcABwDRAQAAiCUAAAAA";
    public static byte[] GetAPKBuffer()
    {
      return Convert.FromBase64String(NOX_SPLASH);
    }

  }
}