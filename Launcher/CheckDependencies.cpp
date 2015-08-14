
#include "stdafx.h"
#include "CheckDependencies.h"

// Global variables
TCHAR installedNet4Version[260];
TCHAR installedVC2010Version[260];
bool net4Installed;
bool kb2468871Installed;
bool vc2010Installed;

void CheckDependencies()
{
	HKEY hKey;
	LONG result;
	DWORD type;
	DWORD size;
	TCHAR szValue[260] = { 0 };
	TCHAR szValue2[260] = { 0 };
	DWORD dwValue = 0;
	DWORD dwValue2 = 0;

	net4Installed = false;
	kb2468871Installed = false;
	vc2010Installed = false;

	result = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client"), 0, KEY_READ, &hKey);
	if (result == ERROR_SUCCESS)
	{
		size = 4;
		result = RegQueryValueEx(hKey, _T("Install"), NULL, &(type = REG_DWORD), (LPBYTE)&dwValue, &size);
		if (result == ERROR_SUCCESS)
		{
			net4Installed = dwValue != 0;
		}
		size = 260;
		result = RegQueryValueEx(hKey, _T("Version"), NULL, &(type = REG_SZ), (LPBYTE)&szValue, &size);
		if (result == ERROR_SUCCESS)
		{
			_tcsncpy_s(installedNet4Version, 260, szValue, size);
		}
	}

	result = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\Updates\\Microsoft .NET Framework 4 Client Profile\\KB2468871"), 0, KEY_READ, &hKey);
	if (result == ERROR_SUCCESS)
	{
		size = 260;
		result = RegQueryValueEx(hKey, _T("ThisVersionInstalled"), NULL, &(type = REG_SZ), (LPBYTE)&szValue, &size);
	}
	result = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\Updates\\Microsoft .NET Framework 4 Client Profile\\KB2468871v2"), 0, KEY_READ, &hKey);
	if (result == ERROR_SUCCESS)
	{
		size = 260;
		result = RegQueryValueEx(hKey, _T("ThisVersionInstalled"), NULL, &(type = REG_SZ), (LPBYTE)&szValue2, &size);
	}
	if ((szValue[0] == 'Y') || (szValue2[0] == 'Y'))
		kb2468871Installed = true;

	if (systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
		result = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\VisualStudio\\10.0\\VC\\VCRedist\\x86"), 0, KEY_READ, &hKey);
	else
		result = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\VisualStudio\\10.0\\VC\\VCRedist\\x64"), 0, KEY_READ, &hKey);
	if (result == ERROR_SUCCESS)
	{
		size = 4;
		result = RegQueryValueEx(hKey, _T("Installed"), NULL, &(type = REG_DWORD), (LPBYTE)&dwValue, &size);
		if (result == ERROR_SUCCESS)
		{
			vc2010Installed = dwValue != 0;
		}
		size = 260;
		result = RegQueryValueEx(hKey, _T("Version"), NULL, &(type = REG_SZ), (LPBYTE)&szValue, &size);
		if (result == ERROR_SUCCESS)
		{
			_tcsncpy_s(installedVC2010Version, 260, szValue, size);
		}
	}
}