
# Compiler Notes


 
=== Visual Studio Enterprise 2019 for Mac ===

Version 8.9.7 (build 8)

Xamarin.Mac 6.18.0.23 (d16-6 / 088c73638)

Package version: 612000125

Mono Runtime: Mono 6.12.0.125 (2020-02/8c552e98bd6) (64-bit) Package version: 612000125

Xamarin.Android Version: 11.2.2.1 (Visual Studio Enterprise)

Supported Android versions: 8.1 (API level 27)

SDK Tools Version: 26.1.1SDK

Platform Tools Version: 30.0.4SDK

Build Tools Version: 30.0.2

NDK Version: 21.3.6528147


# COSU Notes

BitfiWallet implements device owner policies and persistent lock tasks to the extent of what is offered by [DPM SDK 27](https://developer.android.com/reference/android/app/admin/DevicePolicyManager) and does not enjoy the same privileges of a system package.

_____

We cannot uninstall or disable a system package but are able to enforce the desired outcome by making [these](https://github.com/Bitfi/BitfiWallet/blob/main/NoxDevice/ConfigValues.cs#L36) suspended and hidden; we can certify that existence of non BitfiWallet traffic effectively limited to DNS and network time.

To suspend certain system packages we must become direct boot aware, in the absence of a direct boot task system will use com.android.settings and com.android.launcher3...seen before as "Wallet is starting". BitfiWallet is rather large so for our task to start instantly com.rokits.direct is deployed and we delegate with DPM so it may persist across reboots. This is a [tiny apk](https://github.com/Bitfi/BitfiWallet/blob/main/NoxDevice/XSplash_Zero.cs) created with a single blank view which also serves to keep our task alive during the couple seconds between a successful update and process starting again.


# Special Thanks

- Nicolas Dorier (@NicolasDorier)

It's fair to say that Bitfi would not have been conceived if not for Nicolas and NBitcoin, getting started with Bitcoin would have been seriously daunting in the absence of the library he created, perfected and so wonderfully documented. Nicolas has acquired the combined expertise in .Net (our c# framework) and blockchain development to such advanced extent that is second to none, and though not part of this project offers special capacity to support future blockchain innovation through his continued work with NBitcoin.
