# Copyright (C) 2018 ycmd contributors
#
# This file is part of ycmd.
#
# ycmd is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# ycmd is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with ycmd.  If not, see <http://www.gnu.org/licenses/>.

cmake_minimum_required( VERSION 2.8.11 )

project( _regex C )

# Silence the warning about CMake policy CMP0042
set( CMAKE_MACOSX_RPATH ON )

find_package( PythonLibs )

if ( PYTHONLIBS_VERSION_STRING VERSION_LESS "3.0.0" )
  set( REGEX_MODULE_DIR ${PROJECT_SOURCE_DIR}/regex_2 )
else()
  set( REGEX_MODULE_DIR ${PROJECT_SOURCE_DIR}/regex_3 )
endif()

file( GLOB_RECURSE REGEX_SOURCES ${REGEX_MODULE_DIR}/*.h
                                 ${REGEX_MODULE_DIR}/*.c )

include_directories( SYSTEM
                     ${PYTHON_INCLUDE_DIRS} )

add_library( ${PROJECT_NAME} SHARED
             ${REGEX_SOURCES} )

target_link_libraries( ${PROJECT_NAME}
                       ${PYTHON_LIBRARIES} )

# We don't want the "lib" prefix, it can screw up python when it tries to search
# for our module
set_target_properties( ${PROJECT_NAME} PROPERTIES PREFIX "" )

if ( WIN32 OR CYGWIN OR MSYS )
  # DLL platforms put dlls in the RUNTIME_OUTPUT_DIRECTORY
  # First for the generic no-config case (e.g. with mingw)
  set_target_properties( ${PROJECT_NAME} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY ${REGEX_MODULE_DIR} )
  # Second, for multi-config builds (e.g. msvc)
  foreach( OUTPUTCONFIG ${CMAKE_CONFIGURATION_TYPES} )
    string( TOUPPER ${OUTPUTCONFIG} OUTPUTCONFIG )
    set_target_properties( ${PROJECT_NAME} PROPERTIES
                           RUNTIME_OUTPUT_DIRECTORY_${OUTPUTCONFIG}
                           ${REGEX_MODULE_DIR} )
  endforeach()

  if ( WIN32 )
    # This is the extension for compiled Python modules on Windows
    set_target_properties( ${PROJECT_NAME} PROPERTIES SUFFIX ".pyd")
  elseif ( CYGWIN OR MSYS )
    # This is the extension for compiled Python modules in Cygwin and msys
    set_target_properties( ${PROJECT_NAME} PROPERTIES SUFFIX ".dll")
  endif()
else()
  # Even on macs, we want a .so extension instead of a .dylib which is what
  # cmake would give us by default. Python won't recognize a .dylib as a module,
  # but it will recognize a .so
  set_target_properties( ${PROJECT_NAME} PROPERTIES SUFFIX ".so")
endif()

set_target_properties( ${PROJECT_NAME} PROPERTIES
    LIBRARY_OUTPUT_DIRECTORY ${REGEX_MODULE_DIR} )
