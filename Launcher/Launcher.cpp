// Network_Manager_Launcher.cpp : Defines the entry point for the application.
// TODO: Add/migrate launcher to single exe setup file

#include "stdafx.h"
#include "CheckDependencies.h"
#include "Network.h"
#include <ShellAPI.h>
#include "resource.h"
#include "Launcher.h"
#include <regex>
#include <stdio.h>
#include <iostream>
#include <Commctrl.h>
#pragma comment (lib, "Comctl32.lib")
#pragma comment (lib, "Shell32.lib")
#pragma comment (lib, "Version.lib")

#define MAIN 1
#define MAX_LOADSTRING 100
#define IDC_NET4 1
#define IDC_NET4_KB 2
#define IDC_VC2010 3
#define IDC_REFRESH 4

using namespace std;

// Global Variables:
HINSTANCE hInst;								// current instance
TCHAR szTitle[MAX_LOADSTRING];					// The title bar text
TCHAR szWindowClass[MAX_LOADSTRING];			// the main window class name
HWND hNet4Installed;
HWND hNet4KBInstalled;
HWND hVC2010Installed;
HWND hNet4Button;
HWND hNet4KBButton;
HWND hVC2010Button;
HWND hMainWindow;
wstring ownPath;
SYSTEM_INFO systemInfo;							// OS bitness version
string userAgent;								// HTTP User-Agent
//HMODULE hNtdll = GetModuleHandle(_T("ntdll.dll"));
//Crc32 ComputeCrc32; // = (Crc32)GetProcAddress(hNtdll, "RtlComputeCrc32");



// Forward declarations of functions included in this code module:
ATOM				MyRegisterClass(HINSTANCE hInstance);
BOOL				InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);
void				RefreshGui();
void				DependecyDownloaded(bool, wstring);
void				RunNM();
BOOL				IsUserAdmin(VOID);

int APIENTRY _tWinMain(_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPTSTR    lpCmdLine,
	_In_ int       nCmdShow)
{
	InitCommonControls();
	GetNativeSystemInfo(&systemInfo);
	CheckDependencies();
	WCHAR ownFilePath[32767];
	GetModuleFileNameW(hInst, ownFilePath, (sizeof(ownFilePath)));
	ownPath = regex_replace(wstring(ownFilePath), std::wregex(L"[^\\\\]+$"), L"");
	_wchdir(ownPath.c_str());
	if (net4Installed && kb2468871Installed) // && vc2010Installed)
	{
		RunNM();
		return FALSE;
	}
	if (!IsUserAdmin())
	{
		MessageBoxA(NULL, "The current logged in Windows user is not an administrator. This program can only be run as an administrator.\nThe program won't work properly.", "User Permissions", MB_OK);
		return FALSE;
	}

	OSVERSIONINFOEX osVersionInfo;
	osVersionInfo.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	GetVersionEx((LPOSVERSIONINFO)&osVersionInfo);
	TCHAR szExeFileName[MAX_PATH];
	GetModuleFileName(NULL, szExeFileName, MAX_PATH);
	int versionInfoSize = GetFileVersionInfoSize(szExeFileName, nullptr);
	char* buffer = new char[versionInfoSize];
	GetFileVersionInfo(szExeFileName, NULL, versionInfoSize, buffer);
	VS_FIXEDFILEINFO* pFixedInfo;
	UINT pLen;
	VerQueryValue(buffer, _T("\\"), (LPVOID*)&pFixedInfo, &pLen);
	userAgent = "SB Network Manager Launcher ";
	userAgent.append(std::to_string(pFixedInfo->dwFileVersionMS >> 16));
	userAgent.append(".");
	userAgent.append(std::to_string(pFixedInfo->dwFileVersionMS & 0xffff));
	userAgent.append(".");
	userAgent.append(std::to_string(pFixedInfo->dwFileVersionLS >> 16));
	userAgent.append(".");
	userAgent.append(std::to_string(pFixedInfo->dwFileVersionLS & 0xffff));
	userAgent.append(" (Windows NT ");
	userAgent.append(std::to_string(osVersionInfo.dwMajorVersion));
	userAgent.append(".");
	userAgent.append(std::to_string(osVersionInfo.dwMinorVersion));
	userAgent.append(".");
	userAgent.append(std::to_string(osVersionInfo.dwBuildNumber));
	userAgent.append(".");
	userAgent.append(std::to_string(osVersionInfo.wServicePackMajor << 16 | osVersionInfo.wServicePackMinor));
	userAgent.append((systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL) ? " 32-bit)" : " 64-bit)");
	delete[] buffer;

	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// TODO: Place code here.
	MSG msg;
	HACCEL hAccelTable;

	// Initialize global strings
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
	LoadString(hInstance, IDC_NETWORK_MANAGER_LAUNCHER, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// Perform application initialization:
	if (!InitInstance(hInstance, nCmdShow))
	{
		return FALSE;
	}

	hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_NETWORK_MANAGER_LAUNCHER));

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0))
	{
		if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	return (int)msg.wParam;
}



//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEX wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc;
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInstance;
	wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_NETWORK_MANAGER_LAUNCHER));
	wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = MAKEINTRESOURCE(IDC_NETWORK_MANAGER_LAUNCHER);
	wcex.lpszClassName = szWindowClass;
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_ICON1));

	return RegisterClassEx(&wcex);
}

//
//   FUNCTION: InitInstance(HINSTANCE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{


	hInst = hInstance; // Store instance handle in our global variable

	hMainWindow = CreateWindow(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, 0, 700, 250, NULL, NULL, hInstance, NULL);

	if (!hMainWindow)
	{
		return FALSE;
	}

	CreateDirectory(_T("Downloads"), NULL);
	ShowWindow(hMainWindow, nCmdShow);
	RefreshGui();
	return TRUE;
}

void RefreshGui()
{
	CheckDependencies();
	if (net4Installed && kb2468871Installed) // && vc2010Installed)
	{
		RunNM();
		exit(EXIT_SUCCESS);
	}
	TCHAR text[100] = { 0 };
	if (net4Installed)
	{
		_tcscat_s(text, 100, installedNet4Version);
		_tcscat_s(text, 100, _T(" installed"));
		SetWindowText(hNet4Installed, text);
		ShowWindow(hNet4Button, SW_HIDE);
	}
	else
	{
		text[0] = 0; text[1] = 1;
		_tcscat_s(text, 100, _T("Not installed"));
		SetWindowText(hNet4Installed, text);
		ShowWindow(hNet4Button, SW_SHOW);
	}
	if (kb2468871Installed)
	{
		text[0] = 0; text[1] = 1;
		_tcscat_s(text, 100, _T("Installed"));
		SetWindowText(hNet4KBInstalled, text);
		ShowWindow(hNet4KBButton, SW_HIDE);
	}
	else
	{
		text[0] = 0; text[1] = 1;
		_tcscat_s(text, 100, _T("Not installed"));
		SetWindowText(hNet4KBInstalled, text);
		if (net4Installed)
			ShowWindow(hNet4KBButton, SW_SHOW);
		else
			ShowWindow(hNet4KBButton, SW_HIDE);
	}
	/*if (vc2010Installed)
	{
		text[0] = 0; text[1] = 1;
		_tcscat_s(text, 100, installedVC2010Version);
		_tcscat_s(text, 100, _T(" installed"));
		SetWindowText(hVC2010Installed, text);
		ShowWindow(hVC2010Button, SW_HIDE);
	}
	else
	{
		text[0] = 0; text[1] = 1;
		_tcscat_s(text, 100, _T("Not installed"));
		SetWindowText(hVC2010Installed, text);
		ShowWindow(hVC2010Button, SW_SHOW);
	}*/
	UpdateWindow(hMainWindow);
}

void RunNM()
{
	FILE *file = NULL;
	char buffer[512];
	ZeroMemory(&buffer, 512);
	unsigned int crc32 = 0;
	unsigned int lastCrc32 = 0;
	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	if (_wfopen_s(&file, L"Downloads\\Launcher.exe", L"rb") == 0)
	{
		fclose(file);
		char *param = "/v:on /c "
			"taskkill /F /IM Launcher.exe & "
			"copy /y Downloads\\License.txt License.txt & "
			"for /l %i in (0,1,20) do ( "
			"copy /y Downloads\\Launcher.exe Launcher.exe & "
			"if [!errorlevel!]==[0] ( "
			"del /f /q Downloads\\Launcher.exe & "
			"start Launcher.exe & exit ) "
			"else ( ping -n 2 127.0.0.1>nul ) )";
		ShellExecuteA(NULL, "open", "cmd.exe", param, NULL, SW_HIDE);
		exit(ERROR_SUCCESS);
	}
	//if (_wfopen_s(&file, L"Downloads\\Network_Manager.crc", L"rb") == 0)
	//{
	//	DWORD size;
	//	ZeroMemory(&buffer, 512);
	//	fread_s(&buffer, 9, 1, 8, file);
	//	crc32 = strtoul(buffer, nullptr, 16);
	//	lastCrc32 = 0;
	//	fclose(file);
	//	if (_wfopen_s(&file, L"Downloads\\Network_Manager.exe", L"rb") == 0)
	//	{
	//		while ((size = fread_s(buffer, 512, 1, 512, file)) > 0)
	//			lastCrc32 = ComputeCrc32(lastCrc32, (BYTE*)&buffer, size);
	//		fclose(file);
	//		if (lastCrc32 == crc32)
	//		{
	//			char *param = "/v:on /c "
	//				"taskkill /F /IM Network_Manager.exe & "
	//				"for /l %i in (0,1,20) do ( "
	//				"copy /y Downloads\\Network_Manager.exe Network_Manager.exe & "
	//				"if [!errorlevel!]==[0] ( "
	//				"del /f /q Downloads\\Network_Manager.exe & "
	//				"del /f /q Downloads\\Network_Manager.crc & "
	//				"exit ) "
	//				"else ( ping -n 2 127.0.0.1>nul ) )";
	//			ShellExecuteA(NULL, "RunAs", "cmd.exe", param, NULL, SW_HIDE);
	//			/*FILE *pipe = _popen("tasklist", "r");
	//			string result = "";
	//			while (!feof(pipe))
	//			{
	//				if (fgets(buffer, 512, pipe) != NULL)
	//					result += buffer;
	//			}
	//			MessageBoxA(NULL, result.c_str(), "", MB_OK);*/
	//		}

	//	}
	//}
	// suppress dependency missing error for PcapDotNet
	SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOOPENFILEERRORBOX);
	ZeroMemory(&si, sizeof(STARTUPINFO));
	si.cb = sizeof(STARTUPINFO);
	ZeroMemory(&pi, sizeof(PROCESS_INFORMATION));
	if (systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
		CreateProcessW(L"Network_Manager_x86.exe", NULL, NULL, NULL, false, 0, NULL, NULL, &si, &pi);
	else
		CreateProcessW(L"Network_Manager_x64.exe", NULL, NULL, NULL, false, 0, NULL, NULL, &si, &pi);
}

void DownloadDependency(int idc)
{
	TCHAR *link;
	wstring file;
	string res;
	DWORDLONG conditionMask = 0;
	switch (idc)
	{
	case IDC_NET4:
		OSVERSIONINFOEX osVersionInfo;
		osVersionInfo.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
		osVersionInfo.dwMajorVersion = 6;
		osVersionInfo.dwMinorVersion = 0;
		VER_SET_CONDITION(conditionMask, VER_MAJORVERSION, VER_GREATER_EQUAL);
		VER_SET_CONDITION(conditionMask, VER_MINORVERSION, VER_GREATER_EQUAL);
		if (VerifyVersionInfo(&osVersionInfo, VER_MAJORVERSION | VER_MINORVERSION, conditionMask))
		{
			link = _T("http://download.sortbyte.com/nm/dotnet45");
			file.append(ownPath).append(L"Downloads\\dotnetfx45_full_x86_x64.exe");
		}
		else
		{
			link = _T("http://download.sortbyte.com/nm/dotnet40");
			file.append(ownPath).append(L"Downloads\\dotNetFx40_Full_x86_x64.exe");
		}
		break;
	case IDC_NET4_KB:
		if (systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
		{
			file.append(ownPath).append(L"Downloads\\NDP40-KB2468871-v2-x86.exe");
			link = _T("http://download.sortbyte.com/nm/kb2468871v2_32");
		}
		else
		{
			file.append(ownPath).append(L"Downloads\\NDP40-KB2468871-v2-x64.exe");
			link = _T("http://download.sortbyte.com/nm/kb2468871v2_64");
		}
		break;
	case IDC_VC2010:
		if (systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
		{
			file.append(ownPath).append(L"Downloads\\vcredist_x86.exe");
			link = _T("http://download.sortbyte.com/nm/vcredist2010_32");
		}
		else
		{
			file.append(ownPath).append(L"Downloads\\vcredist_x64.exe");
			link = _T("http://download.sortbyte.com/nm/vcredist2010_64");
		}
		break;
	default:
		link = _T("");
		break;
	}
	res = DownloadToVar(link, false);
	if (regex_match(res, std::regex("[\\da-f]+\n[\\d]+\\s*", std::regex_constants::icase)))
	{
		std::smatch match;
		regex_search(res, match, std::regex("^[\\da-f]+(?=\n)"));
		unsigned int crc32 = stoul(match[0], nullptr, 16);
		unsigned long long size = stoull(regex_replace(res, std::regex("^[\\da-f]+\n([\\d]+)\\s*$", std::regex_constants::icase), "$1"), nullptr, 10);
		Download(link, file, crc32, size, DependecyDownloaded);
	}
	else
		MessageBox(NULL, _T("Failed to download the update.\nBroken link or no internet access !"), _T("Download"), MB_OK);

}

void DependecyDownloaded(bool succes, wstring file)
{
	if (succes)
	{
		STARTUPINFO startUpInfo;
		ZeroMemory(&startUpInfo, sizeof(STARTUPINFO));
		startUpInfo.cb = sizeof(STARTUPINFO);
		PROCESS_INFORMATION process_information;
		ZeroMemory(&process_information, sizeof(PROCESS_INFORMATION));
		CreateProcessW(file.c_str(), NULL, NULL, NULL, false, 0, NULL, NULL, &startUpInfo, &process_information);
	}
	else
	{
		MessageBoxA(NULL, "Download was corrupted, interrupted or already started !", "Download", MB_OK);
	}
}

//
//  FUNCTION: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND	- process the application menu
//  WM_PAINT	- Paint the main window
//  WM_DESTROY	- post a quit message and return
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	int wmId, wmEvent;
	PAINTSTRUCT ps;
	HDC hdc;
	TCHAR *text;

	switch (message)
	{
	case WM_CREATE:
		text = _T("You are missing one or more dependencies required to perform this action.\nBellow is the list of required packages / libraries. Install the ones that are missing !");
		CreateWindow(_T("STATIC"), text, SS_LEFT | WS_CHILD | WS_VISIBLE, 10, 10, 600, 60, hWnd, NULL, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);


		text = _T("Microsoft .NET Framework 4");
		CreateWindow(_T("STATIC"), text, SS_LEFT | WS_CHILD | WS_VISIBLE, 10, 70, 320, 20, hWnd, NULL, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);
		hNet4Installed = CreateWindow(_T("STATIC"), _T("Checking ..."), SS_LEFT | WS_CHILD | WS_VISIBLE, 330, 70, 170, 20, hWnd, NULL, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);
		hNet4Button = CreateWindow(_T("BUTTON"), _T("Download && Install"), BS_PUSHBUTTON | WS_CHILD | WS_VISIBLE, 500, 70, 150, 20, hWnd, (HMENU)IDC_NET4, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);

		text = _T("Microsoft .NET Framework 4 KB2468871");
		CreateWindow(_T("STATIC"), text, SS_LEFT | WS_CHILD | WS_VISIBLE, 10, 90, 320, 20, hWnd, NULL, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);
		hNet4KBInstalled = CreateWindow(_T("STATIC"), _T("Checking ..."), SS_LEFT | WS_CHILD | WS_VISIBLE, 330, 90, 170, 20, hWnd, NULL, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);
		hNet4KBButton = CreateWindow(_T("BUTTON"), _T("Download && Install"), BS_PUSHBUTTON | WS_CHILD | WS_VISIBLE, 500, 90, 150, 20, hWnd, (HMENU)IDC_NET4_KB, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);

		/*if (systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
			text = _T("Microsoft Visual C++ 2010 x86 Redistributable");
		else
			text = _T("Microsoft Visual C++ 2010 x64 Redistributable");
		CreateWindow(_T("STATIC"), text, SS_LEFT | WS_CHILD | WS_VISIBLE, 10, 110, 320, 20, hWnd, NULL, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);
		hVC2010Installed = CreateWindow(_T("STATIC"), _T("Checking ..."), SS_LEFT | WS_CHILD | WS_VISIBLE, 330, 110, 170, 20, hWnd, NULL, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);
		hVC2010Button = CreateWindow(_T("BUTTON"), _T("Download && Install"), BS_PUSHBUTTON | WS_CHILD | WS_VISIBLE, 500, 110, 150, 20, hWnd, (HMENU)IDC_VC2010, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);*/

		CreateWindow(_T("BUTTON"), _T("Refresh"), BS_PUSHBUTTON | WS_CHILD | WS_VISIBLE, 300, 150, 100, 20, hWnd, (HMENU)IDC_REFRESH, (HINSTANCE)GetWindowLong(hWnd, GWL_HINSTANCE), NULL);
		break;
	case WM_COMMAND:
		wmId = LOWORD(wParam);
		wmEvent = HIWORD(wParam);
		// Parse the menu selections:
		switch (wmId)
		{
			/*case IDM_ABOUT:
			DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
			break;
			case IDM_EXIT:
			DestroyWindow(hWnd);
			break;*/
		case IDC_NET4:
			DownloadDependency(IDC_NET4);
			break;
		case IDC_NET4_KB:
			DownloadDependency(IDC_NET4_KB);
			break;
		case IDC_VC2010:
			DownloadDependency(IDC_VC2010);
			break;
		case IDC_REFRESH:
			RefreshGui();
			break;
		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
		}
		break;
	case WM_CTLCOLORSTATIC:

		SetBkMode((HDC)wParam, TRANSPARENT);
		if ((HWND)lParam == hNet4Installed)
		{
			if (net4Installed)
				SetTextColor((HDC)wParam, RGB(0, 0x88, 0));
			else
				SetTextColor((HDC)wParam, RGB(0xFF, 0, 0));
		}
		else if ((HWND)lParam == hNet4KBInstalled)
		{
			if (kb2468871Installed)
				SetTextColor((HDC)wParam, RGB(0, 0x88, 0));
			else
				SetTextColor((HDC)wParam, RGB(0xFF, 0, 0));
		}
		else if ((HWND)lParam == hVC2010Installed)
		{
			if (vc2010Installed)
				SetTextColor((HDC)wParam, RGB(0, 0x88, 0));
			else
				SetTextColor((HDC)wParam, RGB(0xFF, 0, 0));
		}
		break;
	case WM_PAINT:
		hdc = BeginPaint(hWnd, &ps);
		// TODO: Add any drawing code here...
		EndPaint(hWnd, &ps);
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}
	return 0;
}

BOOL IsUserAdmin(VOID)
/*++
Routine Description: This routine returns TRUE if the caller's
process is a member of the Administrators local group. Caller is NOT
expected to be impersonating anyone and is expected to be able to
open its own process and process token.
Arguments: None.
Return Value:
TRUE - Caller has Administrators local group.
FALSE - Caller does not have Administrators local group. --
*/
{
	BOOL b;
	SID_IDENTIFIER_AUTHORITY NtAuthority = SECURITY_NT_AUTHORITY;
	PSID AdministratorsGroup;
	b = AllocateAndInitializeSid(
		&NtAuthority,
		2,
		SECURITY_BUILTIN_DOMAIN_RID,
		DOMAIN_ALIAS_RID_ADMINS,
		0, 0, 0, 0, 0, 0,
		&AdministratorsGroup);
	if (b)
	{
		if (!CheckTokenMembership(NULL, AdministratorsGroup, &b))
		{
			b = FALSE;
		}
		FreeSid(AdministratorsGroup);
	}

	return(b);
}