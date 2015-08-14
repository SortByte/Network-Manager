#pragma once
#include <string>

void Download(TCHAR *url, std::wstring filePath, unsigned int crc32, unsigned long long length, void(*callback)(bool, std::wstring));
std::string DownloadToVar(TCHAR*, bool = true);