
# Contributor Wishlist


With local updates we aim to allow installation of firmware compiled by leading industry professionals, those with a stellar reputation and capacity to ensure nothing violates user trust. It's our opinion that a package compiled by any one of these developers is an excellent choice for the desired validation, we sincerely hope for their involvement and encourage users to respectfully donate in appreciation for the continued privilege of running their builds.


- Andrew Tierney (@cybergibbons)

Andrew is the security researcher with an established record proving he should be the obvious choice for any user seeking an independent professional to certify our work. There is no doubt that Andrew will continue to zeal for the security of all users, and we can appreciate what's in devices today because of his historical assessments of Bitfi security. Users can expect Andrew to leave no stone unturned in a thorough examination before offering his build.

- Nicolas Dorier (@NicolasDorier)

It's fair to say that Bitfi would not have been conceived if not for Nicolas and NBitcoin, getting started with Bitcoin would have been seriously daunting in the absence of the library he created, perfected and so wonderfully documented. Nicolas has acquired the combined expertise in .Net (our c# framework) and blockchain development to such advanced extent that is second to none, with special capacity to review this implementation and support future innovation.

- Havoc Pennington (@havocp)

We need not speak for Havocs' contribution to computer science, though will say that if having the opportunity to meet you'll be delighted to exchange ideas with a truly accomplished engineer who treats all aspiring developers as his equal. We'd be thrilled with his acceptance in this constitution and believe Havocs' involvement, in any capacity, will bring desirable governance and help us navigate with a fresh new outlook of consummate professionalism.

- Eric Sauvageau (@RMerlinDev)

Thousands of people have swapped official firmware on Asus routers for superior alternatives built and maintained by Asuswrt-Merlin, for faster and safer networks trusted by even the smartest of engineers, including those who are still on the fence with Bitcoin but say we can trust Eric implicitly. The achievement with firmware adopted by so many of us is certainly remarkable, we all want the Bitfi Merlin build and can't wait to see what he might do with our device.


______



Public keys for participating professionals will need prior reference in what the device is running; we are not suggesting a multi-sig approach since deterministic builds are very unlikely here. To be clear, by choosing these builds a user is electing new trust and authority over what is installed.



# Compiler Notes


 
=== Visual Studio Enterprise 2019 for Mac ===

Version 8.9.7 (build 8)

Xamarin.Mac 6.18.0.23 (d16-6 / 088c73638)

Package version: 612000125

Mono Runtime:	Mono 6.12.0.125 (2020-02/8c552e98bd6) (64-bit)	Package version: 612000125

Xamarin.Android Version: 11.2.2.1 (Visual Studio Enterprise)

Supported Android versions:		8.1 (API level 27)

SDK Tools Version: 26.1.1SDK

Platform Tools Version: 30.0.4SDK

Build Tools Version: 30.0.2

NDK Version: 21.3.6528147


# COSU Notes

BitfiWallet implements device owner policies and persistent lock tasks to the extent of what is offered by [DPM SDK 27](https://developer.android.com/reference/android/app/admin/DevicePolicyManager) and does not enjoy the same privileges of a system package.

We cannot uninstall or disable a system package but are able to enforce the desired outcome by making [these](https://github.com/Bitfi/BitfiWallet/blob/main/NoxDevice/ConfigValues.cs#L36) suspended and hidden; we can certify that existence of non BitfiWallet traffic effectively limited to DNS and network time.

To suspend certain system packages we must become direct boot aware, in the absence of a direct boot task system will use com.android.settings and com.android.launcher3...seen before as "Wallet is starting". BitfiWallet is rather large so for our task to start instantly com.rokits.direct is deployed and we delegate with DPM so it may persist across reboots. This is a [tiny apk](https://github.com/Bitfi/BitfiWallet/blob/main/NoxDevice/XSplash_Zero.cs) created with a single blank view which also serves to keep our task alive during the couple seconds between a successful update and process starting again.
