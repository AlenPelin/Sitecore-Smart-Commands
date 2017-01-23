# Sitecore Smart Commands

Sitecore Smart Commands is a shared-source module that contains smart copy, duplicate and clone commands in Content Editor. 

## Download

You can find downloads in github releases tab.

### Known issues

Most of the features (`1`, `2`, `3`) do not support Item Buckets out of the box and may break this functionality so please **dot not install this module if you use Item Buckets or going to use them**. However you can still use this module API and code to implement you own needs. 

### Why is this solution better?

Comparing to any other solutions, this one uses stock Sitecore link replacement functionality instead of custom logic. Have you ever seen the `Breaking links` dialog which offers:

* delete links
* keep links broken
* change links to point to another item

So this module uses same API as the `Breaking links` dialog.

##  Features

#### 1. Smart Duplicate

The `Smart Duplicate` button that does the same thing as normal `Duplicate Item`, but in addition to that it replaces links from source item (and its descendants) to newly created item (and its descendants). Note, it does not work with item buckets.

```
Having this item tree

  sourceItem: linkField -> sourceChild 
    sourceChild: linkField -> sourceItem

by default, when duplicating it the result becomes

  copyItem: linkField -> sourceChild
    copyChild: linkField -> sourceItem

But with Smart Duplicate it becomes

  copyItem: linkField -> copyChild
    copyChild: linkField -> copyItem
```

#### 2. Smart Copy To

Same as Smart Duplicate, but new item appears in another folder.

#### 3. Smart Clone

Same as Smart Copy To, but instead of plain copying the Item Clone is created.

#### 4. Smart Create From Branch

Same idea as with feature number 2, but this time links are expanded for items created from branch templates. 

**Important:** unlike items 1-3 you cannot use original link-agnostic Create From Branch command - create from branch operations will be smart. 

```
Having this branch template

  my branch
    $name: linkField -> branchChild
      branchChild: linkField -> $name

by default, the item created from this branch template becomes

  createdItem: linkField -> branchChild
    createdChild: linkField -> $name

With smart link replacement it becomes expected:

  createdItem: linkField -> createdChild
    createdChild -> createdItem
```
